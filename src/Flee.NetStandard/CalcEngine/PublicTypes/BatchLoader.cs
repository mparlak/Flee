
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Flee.CalcEngine.InternalTypes;
using Flee.InternalTypes;
using Flee.PublicTypes;

namespace Flee.CalcEngine.PublicTypes
{
    public sealed class BatchLoader
    {

        private readonly IDictionary<string, BatchLoadInfo> _myNameInfoMap;

        private readonly DependencyManager<string> _myDependencies;
        internal BatchLoader()
        {
            _myNameInfoMap = new Dictionary<string, BatchLoadInfo>(StringComparer.OrdinalIgnoreCase);
            _myDependencies = new DependencyManager<string>(StringComparer.OrdinalIgnoreCase);
        }

        public void Add(string atomName, string expression, ExpressionContext context)
        {
            Utility.AssertNotNull(atomName, "atomName");
            Utility.AssertNotNull(expression, "expression");
            Utility.AssertNotNull(context, "context");

            BatchLoadInfo info = new BatchLoadInfo(atomName, expression, context);
            _myNameInfoMap.Add(atomName, info);
            _myDependencies.AddTail(atomName);

            ICollection<string> references = this.GetReferences(expression, context);

            foreach (string reference in references)
            {
                _myDependencies.AddTail(reference);
                _myDependencies.AddDepedency(reference, atomName);
            }
        }

        public bool Contains(string atomName)
        {
            return _myNameInfoMap.ContainsKey(atomName);
        }

        internal BatchLoadInfo[] GetBachInfos()
        {
            string[] tails = _myDependencies.GetTails();
            Queue<string> sources = _myDependencies.GetSources(tails);

            IList<string> result = _myDependencies.TopologicalSort(sources);

            BatchLoadInfo[] infos = new BatchLoadInfo[result.Count];

            for (int i = 0; i <= result.Count - 1; i++)
            {
                infos[i] = _myNameInfoMap[result[i]];
            }

            return infos;
        }

        private ICollection<string> GetReferences(string expression, ExpressionContext context)
        {
            IdentifierAnalyzer analyzer = context.ParseIdentifiers(expression);

            return analyzer.GetIdentifiers(context);
        }
    }

}

