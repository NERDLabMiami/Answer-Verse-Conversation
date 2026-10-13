using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace VNEngine
{
    // The on-stage runtime representation of an Actor: RectTransform-driven position/movement,
    // darken/lighten, and the FadingImage crossfade child. Every character shares the same
    // ActorInstance prefab - which character this instance represents is carried entirely by `data`.
    public class ActorInstance : MonoBehaviour
    {
        public Actor data;              // The Actor ScriptableObject this instance represents
        public Actor_Positions position;
        public Transform custom_position;      // Use to place an actor at a very specific position
        private float brigthness_change = 0.05f;

        [HideInInspector]
        public RectTransform rect;
        bool remove_when_out_of_sight = false;  // Called when sliding the actor out


        public Vector3 desired_position = Vector3.zero;    // Smoothly move actor to this position

        [HideInInspector]
        public Image fading_child_image;    // Used when switching character images
        [HideInInspector]
        public Image cur_image;
        [HideInInspector]
        public Vector3 original_scale;
        [HideInInspector]
        public bool is_moving = false;

        // Stable lookup key of the actor this instance represents (proxied from data)
        public string actor_name => data != null ? data.actor_name : "";


        void Awake()
        {
            rect = GetComponent<RectTransform>();
            cur_image = this.GetComponent<Image>();
            original_scale = rect.localScale;

            if (this.transform.childCount > 0)
                fading_child_image = this.transform.GetChild(0).GetComponent<Image>();
        }
        void Start()
        {
            rect = GetComponent<RectTransform>();
        }


        // Assigns which Actor this instance represents and resets its visible sprite to that actor's
        // default expression. Called both on fresh instantiation and when a pooled/disabled instance
        // is reused for a (possibly different) actor.
        public void Apply_Data(Actor new_data)
        {
            data = new_data;
            Reset();
        }


        // Every character's source art is the same resolution, with blank space above the head baked in
        // per character so a shared, unscaled-per-character frame naturally gives correct relative height
        // perspective when placed side by side - so this box is one fixed constant for everyone, not
        // per-character data. This is the designer's own established portrait scale (matches the historical
        // per-character prefab size); actors are NOT meant to fit entirely on screen - ActorManager's
        // actor_floor_y positions them so only the upper body is in frame and the feet fall below the
        // visible screen, matching the "camera framing" already established by the CUSTOM position markers.
        // Width is a generous bound (comfortably wider than any real portrait) so PreserveAspect is always
        // height-constrained and the rendered width just matches the source image's own proportions.
        private static readonly Vector2 portrait_size = new Vector2(2000f, 1767.56f);

        public void Reset()
        {
            remove_when_out_of_sight = false;

            // Defensive: never let a pooled/reused instance inherit a stale desired_position from
            // whatever it was doing last time (e.g. a prior CUSTOM placement) - default to staying put
            // unless the current placement path explicitly assigns a new target right after this.
            desired_position = GetComponent<RectTransform>().localPosition;

            GetComponent<RectTransform>().sizeDelta = portrait_size;

            Sprite default_sprite = data != null ? data.Get_Default_Sprite() : null;
            GetComponent<Image>().color = Color.white;
            GetComponent<Image>().sprite = default_sprite;
            GetComponent<Image>().overrideSprite = default_sprite;
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x),
                                                Mathf.Abs(transform.localScale.y),
                                                Mathf.Abs(transform.localScale.z));
        }


        // Calls a coroutine to fade in this actor over a number of seconds specified by over_time
        public void Fade_In(float over_time)
        {
            this.StopAllCoroutines();
            StartCoroutine(Fade_In_Coroutine(over_time));
        }
        IEnumerator Fade_In_Coroutine(float over_time)
        {
            Image img = this.GetComponent<Image>();
            Color tmp_color = img.color;
            tmp_color.a = 0;
            float value = 0;

            while (value < over_time)
            {
                value += Time.deltaTime;
                tmp_color.a = Mathf.Lerp(0, 1, value / over_time);
                img.color = tmp_color;
                yield return 0;
            }
            yield break;
        }
        // Calls a coroutine to fade out this actor over a number of seconds specified by over_time.
        // Removes the actor from the scene after fading out
        public void Fade_Out(float over_time)
        {
            this.StopAllCoroutines();
            StartCoroutine(Fade_Out_Coroutine(over_time));
        }
        IEnumerator Fade_Out_Coroutine(float over_time)
        {
            if (!ActorManager.exiting_actors.Contains(this))
                ActorManager.exiting_actors.Add(this);

            Image img = this.GetComponent<Image>();
            Color tmp_color = img.color;
            tmp_color.a = 1;
            float value = 0;

            while (value < over_time)
            {
                value += Time.deltaTime;
                tmp_color.a = Mathf.Lerp(1, 0, value / over_time);
                img.color = tmp_color;
                yield return 0;
            }

            // Remove this actor after we're done
            Debug.Log("Removing actor " + actor_name);
            ActorManager.Remove_Actor(this);

            yield break;
        }


        // Calls a coroutine to slide in this actor over a number of seconds specified by over_time
        public void Slide_In(Actor_Positions destination, float over_time)
        {
            rect = GetComponent<RectTransform>();

            ActorManager.Add_Actor_To(this, destination);

            // Set our starting point so we slide into our desired position
            ActorManager.actor_manager.Slide_Start_Position(this, position);
        }
        // Slides the actor out, and remove the actor after it has gone
        public void Slide_Out(float over_time)
        {
            rect = GetComponent<RectTransform>();

            if (!remove_when_out_of_sight)
            {
                if (this.position == Actor_Positions.LEFT)
                {
                    ActorManager.left_actors.Remove(this);
                    desired_position = ActorManager.actor_manager.offscreen_left.localPosition;
                    if (!ActorManager.exiting_actors.Contains(this))
                        ActorManager.exiting_actors.Add(this);
                }
                else if (this.position == Actor_Positions.RIGHT)
                {
                    ActorManager.right_actors.Remove(this);
                    desired_position = ActorManager.actor_manager.offscreen_right.localPosition;
                    if (!ActorManager.exiting_actors.Contains(this))
                        ActorManager.exiting_actors.Add(this);
                }
                else if (this.position == Actor_Positions.CENTER)
                {
                    ActorManager.center_actors.Remove(this);
                    desired_position = ActorManager.actor_manager.offscreen_right.localPosition;
                    if (!ActorManager.exiting_actors.Contains(this))
                        ActorManager.exiting_actors.Add(this);
                }
                else if (this.position == Actor_Positions.CUSTOM)
                {
                    desired_position = ActorManager.Closest_Side_To_Exit(this);
                    if (!ActorManager.exiting_actors.Contains(this))
                        ActorManager.exiting_actors.Add(this);
                }
                desired_position.y = ActorManager.actor_floor_y;

                remove_when_out_of_sight = true;
            }
        }

        public void Darken()
        {
            this.StopAllCoroutines();
            StartCoroutine(Darken_Coroutine());
        }
        IEnumerator Darken_Coroutine()
        {
            float value = 0;
            while (this.GetComponent<Image>().color != Color.gray)
            {
                value += brigthness_change;
                this.GetComponent<Image>().color = Color.Lerp(Color.white, Color.black, value);
                yield return new WaitForSeconds(0.03f);
            }
            yield break;
        }


        public void Lighten()
        {
            if (this.GetComponent<Image>().color != Color.white)
            {
                StopAllCoroutines();
                StartCoroutine(Lighten_Coroutine());
            }
        }
        IEnumerator Lighten_Coroutine()
        {
            float value = 0;
            while (this.GetComponent<Image>().color != Color.white)
            {
                value += brigthness_change;
                this.GetComponent<Image>().color = Color.Lerp(Color.gray, Color.white, value);
                yield return new WaitForSeconds(0.03f);
            }
            yield break;
        }


        // Instantly places the actor at the designated position
        public void Place_At_Position(Actor_Positions destination)
        {
            position = destination;

            ActorManager.Add_Actor_To(this, destination);
            this.rect = this.GetComponent<RectTransform>();
            this.rect.localPosition = this.desired_position;
        }


        public void Update()
        {
            is_moving = false;
            // Move towards desired position if we are not at our desired position

            if (this.rect.localPosition != desired_position)
            {
                this.rect.localPosition = Vector3.Lerp(this.rect.localPosition, desired_position, 3f * Time.deltaTime);

                if (Vector2.Distance(this.rect.localPosition, desired_position) > 1f)
                    is_moving = true;
            }
            // If sliding out, check if we're visible. If we're not visible, remove the actor
            if (remove_when_out_of_sight && Mathf.Abs(this.rect.localPosition.x - desired_position.x) <= 30)
            {
                ActorManager.Remove_Actor(this);
            }
        }
    }
}
