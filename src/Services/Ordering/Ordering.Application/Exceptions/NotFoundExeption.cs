using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ordering.Application.Exceptions
{
  public class NotFoundExeption : ApplicationException
  {
    public NotFoundExeption(string name, object key) : base($"Entity \"{name}\" ({key}) was not found.")
    { }
  }
}
