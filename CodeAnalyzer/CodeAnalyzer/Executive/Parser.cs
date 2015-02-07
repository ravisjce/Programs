using System;
using System.Collections.Generic;
using System.IO;

namespace CodeAnalysis
{
	class CompositionType{
		struct st{}
		st test1;
	}
	
	class AggregatedeType{

	}
	
	class InheritedType {
	}
	
	class UsingType{
	}
	
	class ChildClass: InheritedType
	{
		AggregatedeType p = new AggregatedeType();
		
		public void get(UsingType ut)
		{
            foreach (IRule rule in Rules)
            {
                if (rule.test(semi))
                    break;
				if (!semi.open(file as string))
                {
                    Console.Write("\n  Can't open {0}\n\n", args[0]);
                    return;
                }
            }
			
			try
			{
				while (semi.getSemi())
					parser.parse(semi);
				Console.Write("\n\n  locations table contains:");
			}
			catch (Exception ex)
			{
				Console.Write("\n\n  {0}\n", ex.Message);
			}

		}
	}
}
