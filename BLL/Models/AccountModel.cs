using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Models
{
    public class AccountModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string _token {  get; set; }
        public DateTime _tokenExpirationDate {  get; set; }
    }
}
