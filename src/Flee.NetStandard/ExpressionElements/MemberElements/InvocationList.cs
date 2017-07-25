using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Flee.ExpressionElements.Base;
using Flee.ExpressionElements.MemberElements;
using Flee.InternalTypes;
using Flee.PublicTypes;
using Flee.Resources;


namespace Flee.ExpressionElements.MemberElements
{
    internal class InvocationListElement : ExpressionElement
    {
        private readonly MemberElement _myTail;
        public InvocationListElement(IList elements, IServiceProvider services)
        {
            this.HandleFirstElement(elements, services);
            LinkElements(elements);
            Resolve(elements, services);
            _myTail = (MemberElement)elements[elements.Count - 1];
        }

        /// <summary>
        /// Arrange elements as a linked list
        /// </summary>
        /// <param name="elements"></param>
        private static void LinkElements(IList elements)
        {
            for (int i = 0; i <= elements.Count - 1; i++)
            {
                MemberElement current = (MemberElement)elements[i];
                MemberElement nextElement = null;
                if (i + 1 < elements.Count)
                {
                    nextElement = (MemberElement)elements[i + 1];
                }
                current.Link(nextElement);
            }
        }

        private void HandleFirstElement(IList elements, IServiceProvider services)
        {
            ExpressionElement first = (ExpressionElement)elements[0];

            // If the first element is not a member element, then we assume it is an expression and replace it with the correct member element
            if (!(first is MemberElement))
            {
                ExpressionMemberElement actualFirst = new ExpressionMemberElement(first);
                elements[0] = actualFirst;
            }
            else
            {
                this.ResolveNamespaces(elements, services);
            }
        }

        private void ResolveNamespaces(IList elements, IServiceProvider services)
        {
            ExpressionContext context = (ExpressionContext)services.GetService(typeof(ExpressionContext));
            ImportBase currentImport = context.Imports.RootImport;

            while (true)
            {
                string name = GetName(elements);

                if (name == null)
                {
                    break; // TODO: might not be correct. Was : Exit While
                }

                ImportBase import = currentImport.FindImport(name);

                if (import == null)
                {
                    break; // TODO: might not be correct. Was : Exit While
                }

                currentImport = import;
                elements.RemoveAt(0);

                if (elements.Count > 0)
                {
                    MemberElement newFirst = (MemberElement)elements[0];
                    newFirst.SetImport(currentImport);
                }
            }

            if (elements.Count == 0)
            {
                base.ThrowCompileException(CompileErrorResourceKeys.NamespaceCannotBeUsedAsType, CompileExceptionReason.TypeMismatch, currentImport.Name);
            }
        }

        private static string GetName(IList elements)
        {
            if (elements.Count == 0)
            {
                return null;
            }

            // Is the first member a field/property element?
            var fpe = elements[0] as IdentifierElement;

            return fpe?.MemberName;
        }

        private static void Resolve(IList elements, IServiceProvider services)
        {
            foreach (MemberElement element in elements)
            {
                element.Resolve(services);
            }
        }

        public override void Emit(FleeILGenerator ilg, IServiceProvider services)
        {
            _myTail.Emit(ilg, services);
        }

        public override System.Type ResultType => _myTail.ResultType;
    }
}
