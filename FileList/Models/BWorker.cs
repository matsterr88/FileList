using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileList.Models
{
    class BWorker
    {
        private BWorker()
        {

        }

        private static BackgroundWorker _worker;

        public static BackgroundWorker getInstance()
        {
            if(_worker == null)
            {
                _worker = new BackgroundWorker();
            }

            return _worker;
        }
    }
}
