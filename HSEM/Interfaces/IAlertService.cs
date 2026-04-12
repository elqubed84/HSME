using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.Interfaces
{
    public interface IAlertService
    {
        Task ShowAsync(string title, string message, string cancel);
    }

}
