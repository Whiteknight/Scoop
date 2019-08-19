﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using FluentAssertions.Common;
using FluentAssertions.Primitives;
using Scoop.SyntaxTree;

namespace Scoop.Tests.Utility
{
    public class AstNodeAssertions : ReferenceTypeAssertions<AstNode, AstNodeAssertions>
    {
        public AstNodeAssertions(AstNode node)
            : base(node)
        {
        }

        protected override string Identifier => "AstNode";

        public AndConstraint<AstNodeAssertions> MatchAst(AstNode expected)
        {
            Subject.Should().NotBeNull();
            expected.Should().NotBeNull();
            AssertMatchAst(Subject, expected, "");
            return new AndConstraint<AstNodeAssertions>(this);
        }

        //public AndConstraint<AstNodeAssertions> RoundTrip()
        //{
        //    var asString = Subject.ToString();
        //    AstNode roundTripped = null;
        //    try
        //    {
        //        roundTripped = new Parser().Parse(asString);
        //        AssertMatchAst(Subject, roundTripped, "ROUNDTRIP");
        //        return new AndConstraint<AstNodeAssertions>(this);
        //    }
        //    catch (Exception e)
        //    {
        //        var message = "Expected\n" + asString;
        //        if (roundTripped != null)
        //            message += "\n\nBut got\n" + roundTripped.ToString();
        //        throw new Exception(message, e);
        //    }
        //}

        //public AndConstraint<AstNodeAssertions> PassValidation()
        //{
        //    Subject.Validate().ThrowOnError();
        //    return new AndConstraint<AstNodeAssertions>(this);
        //}

        private void AssertMatchAst(AstNode a, AstNode b, string path)
        {
            if (a == null && b == null)
                return;

            var type = a.GetType();
            type.Should().BeSameAs(b.GetType(), path);
            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (property.Name == nameof(AstNode.Location) && property.PropertyType == typeof(Location))
                    continue;
                //if (property.Name == nameof(ISqlSymbolScopeNode.Symbols) && property.PropertyType == typeof(SymbolTable))
                //    continue;
                if (property.IsIndexer())
                    continue;

                var childPath = path + "/" + property.Name;
                var childA = property.GetValue(a);
                var childB = property.GetValue(b);
                if (childA == null && childB == null)
                    continue;
                childA.Should().NotBeNull(childPath);
                childB.Should().NotBeNull(childPath);

                if (typeof(AstNode).IsAssignableFrom(property.PropertyType))
                {
                    AssertMatchAst(childA as AstNode, childB as AstNode, childPath);
                    continue;
                }
                if (property.PropertyType.IsGenericType && property.PropertyType.IsConstructedGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var elementType = property.PropertyType.GetGenericArguments().First();
                    if (typeof(AstNode).IsAssignableFrom(elementType))
                    {
                        var listA = (childA as IEnumerable).Cast<AstNode>().ToList();
                        var listB = (childB as IEnumerable).Cast<AstNode>().ToList();
                        listA.Count.Should().Be(listB.Count, childPath);
                        for (int i = 0; i < listA.Count; i++)
                        {
                            var itemPath = childPath + "[" + i + "]";
                            AssertMatchAst(listA[i], listB[i], itemPath);
                        }

                        continue;
                    }
                }

                childA.Should().BeEquivalentTo(childB, childPath);
            }
        }
    }
}