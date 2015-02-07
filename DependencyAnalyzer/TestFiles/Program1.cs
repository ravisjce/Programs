using System;
using System.Collections.Generic;
using System.IO;

namespace CodeAnalysis
{

class ParentClass
	{
	
	}
	
class InheritanceClass:ParentClass
	{
		
		public void get(UsingType ut)
		{
		}
	}
	class AggregatedClass
	{
	ParentClass temp = new ParentClass();
	}
	
	class CompositionClass
	{
	struct st
		{
			int test;
		}
		st test1;
	}
	
	class UsingClass
	{
	public void get(ParentClass ut)
		{
		}
	}
	
	}
