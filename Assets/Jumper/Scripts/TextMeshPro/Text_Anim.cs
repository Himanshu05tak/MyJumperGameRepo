using TMPro;
using UnityEngine;
using System.Collections;

namespace Jumper.Rework.Scriptable
{
    public class Text_Anim : MonoBehaviour
    {
        private TextMeshProUGUI m_Text;

        private IEnumerator Start()
        {
            m_Text = gameObject.GetComponent<TextMeshProUGUI>() ?? gameObject.AddComponent<TextMeshProUGUI>();
            int totalVisibleCharacter = m_Text.text.Length;
            int counter = 0;

            while (true)
            {
                int visibleCount = counter % (totalVisibleCharacter + 1);
                m_Text.maxVisibleCharacters = visibleCount;
                if (visibleCount >= totalVisibleCharacter)
                    yield return new WaitForSeconds(1f);
                counter += 1;
                yield return new WaitForSeconds(0.05f);

            }
        }

    }
}

