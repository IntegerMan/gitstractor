using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitStractor.Model;

namespace GitStractor.Writers
{
    public class AuthorConsoleDataWriter : AuthorDataWriter
    {
        protected override void Dispose(bool disposing)
        {
            
        }

        public override void BeginWriting()
        {
        }

        public override void WriteAuthors(IEnumerable<AuthorInfo> authors)
        {
            foreach (AuthorInfo author in authors)
            {
                Console.WriteLine(author);
            }
        }
    }
}
