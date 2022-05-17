using System.Linq;
using System.Text;

namespace WxClient.commons.command
{
    public abstract class Command
    {
        private string TraceNo { get; set; }

        public override string ToString()
        {
            return ObjectDumper.Dump(this);
        }
    }
}