using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LE_Formatter
{
    public class LeCallStackEntry
    {
        public string File { get; set; }
        public string Line { get; set; }
        public string Location { get; set; }

        public HashSet<indexEntry> associatedOrigins = new HashSet<indexEntry>();

        public string Origin { get => string.Join('\n', associatedOrigins.Select(x => x.name).ToArray()); }

        public bool originClickable { get => associatedOrigins.Count > 0 && associatedOrigins.First() != pythonIndexing.unknown && associatedOrigins.First() != pythonIndexing.vanillaGame; }

        public LeCallStackEntry(string f, string l, string lo)
        {
            this.File = f;
            this.Line = l;
            this.Location = lo;
            setAssociatedOrigins();
        }

        public void setAssociatedOrigins()
        {
            associatedOrigins.Clear();

            foreach (string f in pythonIndexing.vanillaGame)
            {
                if (f.Equals(this.File))
                {
                    associatedOrigins.Add(pythonIndexing.vanillaGame);
                    break;
                }
            }

            foreach(indexEntry ie in pythonIndexing.mods)
            {
                foreach (string f in ie)
                {
                    if (f.Equals(this.File))
                    {
                        associatedOrigins.Add(ie);
                        break;
                    }
                }
            }

            if(associatedOrigins.Count == 0)
            {
                associatedOrigins.Add(pythonIndexing.unknown);
            }
        }
    }
}
