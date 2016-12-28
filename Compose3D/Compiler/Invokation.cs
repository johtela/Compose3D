namespace Compose3D.Compiler
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	internal class Invokation : IEquatable<Invokation>
	{
		public readonly string Name;
		public readonly Function Called;
		public readonly string[] Params;

		public Invokation (Function function, params string[] funcParams)
		{
			if (function.FuncParamsCount != funcParams.Length)
				throw new ArgumentException (
					string.Format ("Wrong number of function parameters. Expexted {0}, got {1}.",
						function.FuncParamsCount, funcParams.Length));
			Called = function;
			Params = funcParams;
			Name = function.Name + (++function.instanceCount);
		}

		public void Output (StringBuilder sb, HashSet<Invokation> outputted, bool outputDecls,
			params string[] funcParams)
		{
			foreach (var fun in Called.Invokations)
				if (!outputted.Contains (fun))
					fun.Output (sb, outputted, outputDecls);
			outputted.Add (this);
			if (outputDecls)
				sb.AppendLine (Called.Declarations);
			sb.AppendLine (Called.Code);
		}

		public override int GetHashCode ()
		{
			return Called.GetHashCode () + Params.Sum (p => p.GetHashCode ());
		}

		public bool Equals (Invokation other)
		{
			return Called == other.Called && 
				Params.Zip (other.Params, (p1, p2) => p1[0] != '#' && p2[0] != '#' && p1.Equals (p2))
				.All (same => same);
		}
	}
}
