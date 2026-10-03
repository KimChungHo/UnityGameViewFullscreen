#if UNITY_EDITOR_WIN
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateTrpgSimulator.Editor.GameViewFullscreen
{
    // 다른 프로젝트의 런타임 현지화 서비스 없이 사용할 수 있는 에디터 전용 문자열 테이블입니다.
    internal sealed class GameViewFullscreenStringTable
    {
        private readonly Dictionary<string, Entry> entries = new(StringComparer.Ordinal);
        private readonly SystemLanguage language;

        internal GameViewFullscreenStringTable(TextAsset table, SystemLanguage language)
        {
            this.language = language;
            string[] lines = (table != null ? table.text : string.Empty)
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            for (int index = 1; index < lines.Length; index++)
            {
                string[] columns = lines[index].Split('\t');
                if (columns.Length < 3 || string.IsNullOrWhiteSpace(columns[0]))
                    continue;
                entries[columns[0].Trim()] = new Entry(columns[1].Trim(), columns[2].Trim());
            }
        }

        internal string Get(string key)
        {
            if (!entries.TryGetValue(key, out Entry entry))
                return key;
            return language == SystemLanguage.Korean ? entry.Korean : entry.English;
        }

        private readonly struct Entry
        {
            internal Entry(string korean, string english)
            {
                Korean = korean;
                English = english;
            }

            internal string Korean { get; }
            internal string English { get; }
        }
    }
}
#endif
