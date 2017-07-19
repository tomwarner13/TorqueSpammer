using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorqueSpammer
{
  class Program
  {
    static void Main(string[] args)
    {
      var spammer = new Spammer();
      while (true)
      {
        spammer.Spam();
      }
    }
  }
}
