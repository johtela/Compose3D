namespace Compose3D.Compiler
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Extensions;

	internal class Invocation : IEquatable<Invocation>
	{
		public readonly string Name;
		public readonly Function Called;
		public readonly string[] Params;

		public Invocation (Function function, params string[] funcParams)
		{
			if (function.FuncParams.Length != funcParams.Length)
				throw new ArgumentException (
					string.Format ("Wrong number of function parameters. Expexted {0}, got {1}.",
						function.FuncParams.Length, funcParams.Length));
			Called = function;
			Params = funcParams;
			Name = function.Name + (++function.instanceCount);
		}

		private void OutputRecursive (StringBuilder sb, HashSet<Invocation> outputted, bool outputDecls,
			string[] callingParams)
		{
			foreach (var par in callingParams)
				if (!IsInstantiated (par))
					throw new ParseException ("Trying to output uninstantiated function.");
			foreach (var inv in Called.Invocations)
				if (!outputted.Contains (inv))
					inv.OutputRecursive (sb, outputted, outputDecls, 
						InstantiateParams (inv.Params, callingParams));
			outputted.Add (this);
			if (outputDecls)
				sb.AppendLine (Called.Declarations);
			sb.AppendLine (ReplaceParams (Called.Code, callingParams));
		}

		private bool IsInstantiated (string param)
		{
			return param[0] != '#';
		}

		private string[] InstantiateParams (string[] funcParams, string[] callingParams)
		{
			return (from fp in funcParams
					select IsInstantiated (fp) ?
						 fp :
						 callingParams[int.Parse (fp.Substring (1))])
					.ToArray ();
		}

		private string ReplaceParams (string code, string[] funcParams)
		{
			var sb = new StringBuilder (code);
			sb.Replace ("#0", Name);
			for (int i = 0; i < funcParams.Length; i++)
				sb.Replace ("#" + (i + 1), funcParams[i]);
			return sb.ToString ();
		}

		public void Output (StringBuilder sb, HashSet<Invocation> outputted, bool outputDecls)
		{
			OutputRecursive (sb, outputted, outputDecls, Params);
		}

		public override int GetHashCode ()
		{
			return Called.GetHashCode () + Params.Sum (p => p.GetHashCode ());
		}

		public bool Equals (Invocation other)
		{
			return Called == other.Called && 
				Params.Zip (other.Params, (p1, p2) => IsInstantiated (p1) && IsInstantiated (p2) && p1.Equals (p2))
				.All (Fun.Identity);
		}
	}
}
