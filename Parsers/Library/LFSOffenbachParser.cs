// This file is part of AlarmWorkflow.
// 
// AlarmWorkflow is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// AlarmWorkflow is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with AlarmWorkflow.  If not, see <http://www.gnu.org/licenses/>.

using System;
using AlarmWorkflow.Shared.Core;
using AlarmWorkflow.Shared.Extensibility;

namespace AlarmWorkflow.Parser.Library
{
    [Export("LFSOffenbachParser", typeof(IParser))]
    class LFSOffenbachParser : IParser
    {
        #region Fields

        private readonly string[] _keywords = new string[] {
            "ALARMAUSDRUCK","EINSATZNUMMER","ORT ","STRASSE"
            ,"OBJEKT ","EINSATZPLANNUMMER","DIAGNOSE",
            "EINSATZSTICHWORT","BEMERKUNGEN","DAS FAX WURDE", "AUSDRUCK VOM", "MELDENDE(R)"
            };

        #endregion

        #region IParser Members

        Operation IParser.Parse(string[] lines)
        {
            Operation operation = new Operation();
            CurrentSection section = CurrentSection.AAnfang;
            lines = Utilities.Trim(lines);
            foreach (var line in lines)
            {
                string keyword;
                if (ParserUtility.StartsWithKeyword(line, _keywords, out keyword))
                {
                    switch (keyword)
                    {
                        case "EINSATZNUMMER": { section = CurrentSection.BeNr; break; }
                        case "ORT ": { section = CurrentSection.CEinsatzort; break; }
                        case "STRASSE": { section = CurrentSection.DStraße; break; }
                        case "OBJEKT ": { section = CurrentSection.FObjekt; break; }
                        case "EINSATZPLANNUMMER": { section = CurrentSection.GEinsatzplan; break; }
                        case "DIAGNOSE": { section = CurrentSection.HMeldebild; break; }
                        case "EINSATZSTICHWORT": { section = CurrentSection.JEinsatzstichwort; break; }
                        case "MELDENDE(R)": { section = CurrentSection.LMeldender; break; }
                        case "BEMERKUNGEN": { section = CurrentSection.KHinweis; break; }
                        case "DAS FAX WURDE": { section = CurrentSection.OFaxtime; break; }
                        case "AUSDRUCK VOM": { section = CurrentSection.MEnde; break; }
                    }
                }
                else
                {
                    section = CurrentSection.MEnde;
                }

                switch (section)
                {
                    case CurrentSection.BeNr:
                        operation.OperationNumber = ParserUtility.GetMessageText(line, keyword);
                        break;
                    case CurrentSection.CEinsatzort:
                        string txt = ParserUtility.GetMessageText(line, keyword);
                        operation.Einsatzort.ZipCode = ParserUtility.ReadZipCodeFromCity(txt);
                        operation.Einsatzort.City = txt.Remove(0, operation.Einsatzort.ZipCode.Length).Trim();
                        break;
                    case CurrentSection.DStraße:
                        operation.Einsatzort.Street = ParserUtility.GetMessageText(line, keyword);
                        break;
                    case CurrentSection.FObjekt:
                        operation.Einsatzort.Property = ParserUtility.GetMessageText(line, keyword);
                        break;
                    case CurrentSection.GEinsatzplan:
                        operation.OperationPlan = ParserUtility.GetMessageText(line, keyword);
                        break;
                    case CurrentSection.HMeldebild:
                        operation.Picture = ParserUtility.GetMessageText(line, keyword);
                        break;
                    case CurrentSection.JEinsatzstichwort:
                        operation.Keywords.EmergencyKeyword = ParserUtility.GetMessageText(line, keyword);
                        break;
                    case CurrentSection.KHinweis:
                        operation.Comment = operation.Comment.AppendLine(ParserUtility.GetMessageText(line, keyword));
                        break;
                    case CurrentSection.OFaxtime:
                        operation.Timestamp = ParserUtility.ReadFaxTimestamp(line, DateTime.Now);
                        break;
                    case CurrentSection.MEnde:
                        break;
                }
            }

            return operation;
        }

        #endregion

        #region Nested types

        private enum CurrentSection
        {
            AAnfang,
            BeNr,
            CEinsatzort,
            DStraße,
            FObjekt,
            GEinsatzplan,
            HMeldebild,
            JEinsatzstichwort,
            KHinweis,
            LMeldender,
            MEnde,
            OFaxtime
        }

        #endregion
    }
}
