using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace foreversickWebAppPSQL
{
    [Serializable]
    public class VisibleSignList
    {
        public List<VisibleSign> visibleSigns { get; set; }
        public VisibleSignList() { visibleSigns = new List<VisibleSign>(); }
        public VisibleSignList(List<VisibleSign> visibleSigns)
        {
            this.visibleSigns = visibleSigns;
        }
        public void Add(VisibleSign visibleSign)
        {
            visibleSigns.Add(visibleSign);
        }
    }
    [Serializable]
    public class VisibleSign
    {
        public int id { get; set; }
        public string name { get; set; }
        public string readable_id { get; set; }
        public VisibleSign() { }

        public VisibleSign(int id, string name, string readable_id)
        {
            this.id = id;
            this.name = name;
            this.readable_id = readable_id;
        }
    }
}
