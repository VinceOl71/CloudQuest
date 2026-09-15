using UnityEngine;

namespace CloudQuest
{
    /// <summary>
    /// On-screen prompt: which cloud this level teaches, which key makes it,
    /// and what to do with it once it exists. A learner who has to be told the
    /// controls out loud has not been given a playable level.
    ///
    /// Drawn with the immediate mode GUI so a level needs no canvas set up.
    /// </summary>
    public class LevelHud : MonoBehaviour
    {
        [SerializeField] private LevelProgress progress;
        [SerializeField] private CloudController clouds;
        [SerializeField] private LevelExit exit;

        private GUIStyle heading;
        private GUIStyle body;

        private void OnGUI()
        {
            if (heading == null)
            {
                heading = new GUIStyle(GUI.skin.label);
                heading.fontSize = 20;
                heading.fontStyle = FontStyle.Bold;
                heading.normal.textColor = Color.white;

                body = new GUIStyle(GUI.skin.label);
                body.fontSize = 15;
                body.wordWrap = true;
                body.normal.textColor = Color.white;
            }

            GUI.Box(new Rect(12, 12, 440, 132), GUIContent.none);

            if (progress != null)
            {
                GUI.Label(new Rect(24, 18, 420, 28),
                          "Level " + progress.Level + " - " + CloudScience.DisplayName(progress.NewCloud),
                          heading);
                GUI.Label(new Rect(24, 46, 416, 56), CloudScience.Description(progress.NewCloud), body);
            }

            GUI.Label(new Rect(24, 104, 416, 22), Controls(), body);

            if (exit != null && exit.Reached)
            {
                GUIStyle done = new GUIStyle(heading);
                done.fontSize = 34;
                done.alignment = TextAnchor.MiddleCenter;
                GUI.Label(new Rect(0, Screen.height * 0.35f, Screen.width, 60), "Level complete", done);
            }
        }

        private string Controls()
        {
            if (clouds == null)
            {
                return "A D move    Space jump";
            }

            System.Text.StringBuilder keys = new System.Text.StringBuilder();
            System.Collections.Generic.IList<CloudType> formable = clouds.FormableClouds();
            for (int i = 0; i < formable.Count; i++)
            {
                keys.Append(i + 1).Append(' ').Append(CloudScience.DisplayName(formable[i])).Append("   ");
            }

            return keys.ToString() + "E grow   Q clear   A D move   Space jump";
        }
    }
}
