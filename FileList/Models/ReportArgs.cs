using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileList.Models
{
    class ReportArgs : EventArgs
    {
        public ReportArgs()
        {
            Count = 0;
            Message = "";
        }

        public ReportArgs(long count, string message)
        {
            Count = count;
            Message = message;
        }

        public ReportArgs(string message)
        {            
            Message = message;
        }
        private long _count;

        public long Count {
            get { return _count; }
            set { _count = value; }
        }

        private string _message;

        public string Message {
            get { return _message; }
            set { _message = value; }
        }

    }
}
