﻿using System;
using System.Collections.Generic;
using System.Linq;
using Scoop.Tokenization;

namespace Scoop.Parsers
{
    public static class ParserMethods
    {
        /// <summary>
        /// Get a reference to a parser. Avoids circular dependencies in the grammar
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="getParser"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Deferred<TInput, TOutput>(Func<IParser<TInput, TOutput>> getParser)
        {
            return new DeferredParser<TInput, TOutput>(getParser);
        }


        /// <summary>
        /// Return the reuslt of the first parser which succeeds
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="parsers"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> First<TInput, TOutput>(params IParser<TInput, TOutput>[] parsers)
        {
            return new FirstParser<TInput, TOutput>(parsers);
        }

        /// <summary>
        /// Parse a list of zero or more items.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="p"></param>
        /// <param name="produce"></param>
        /// <param name="atLeastOne"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> List<TInput, TItem, TOutput>(IParser<TInput, TItem> p, Func<IReadOnlyList<TItem>, TOutput> produce, bool atLeastOne = false)
        {
            return new ListParser<TInput, TItem, TOutput>(p, produce, atLeastOne);
        }

        public static IParser<T, T> Match<T>(Func<T, bool> predicate)
        {
            return new PredicateParser<T, T>(predicate, t => t);
        }

        /// <summary>
        /// Attempt to parse an item and return a default value otherwise
        /// </summary>
        /// <param name="p"></param>
        /// <param name="getDefault"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Optional<TInput, TOutput>(IParser<TInput, TOutput> p, Func<TOutput> getDefault = null)
        {
            return new OptionalParser<TInput, TOutput>(p, getDefault);
        }

        /// <summary>
        /// Produce an empty or default node without consuming anything out of the tokenizer
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="produce"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Produce<TInput, TOutput>(Func<TOutput> produce)
        {
            return new ProduceParser<TInput, TOutput>(t => produce());
        }

        public static IParser<TInput, TOutput> Produce<TInput, TOutput>(Func<ISequence<TInput>, TOutput> produce)
        {
            return new ProduceParser<TInput, TOutput>(produce);
        }

        /// <summary>
        /// Serves as a placeholder in the parser tree where we can make a replacement later.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="defaultParser"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Replaceable<TInput, TOutput>(IParser<TInput, TOutput> defaultParser = null)
        {
            return new ReplaceableParser<TInput, TOutput>(defaultParser ?? new FailParser<TInput, TOutput>());
        }

        /// <summary>
        /// Parse a required item. If the parse fails, produce a default version and attach a
        /// diagnostic error message
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="p"></param>
        /// <param name="produce"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Required<TInput, TOutput>(IParser<TInput, TOutput> p, Func<ISequence<TInput>, TOutput> produce)
        {
            return new RequiredParser<TInput, TOutput>(p, produce);
        }

        /// <summary>
        /// Parse a list of items separated by some separator production
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="TSeparator"></typeparam>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="p"></param>
        /// <param name="separator"></param>
        /// <param name="produce"></param>
        /// <param name="atLeastOne"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> SeparatedList<TInput, TItem, TSeparator, TOutput>(IParser<TInput, TItem> p, IParser<TInput, TSeparator> separator, Func<IReadOnlyList<TItem>, TOutput> produce, bool atLeastOne = false)
        {
            if (atLeastOne)
            {
                return Sequence(
                    p,
                    List(
                        Sequence(
                            separator,
                            p,
                            (s, item) => item
                        ),
                        items => items
                    ),
                    (first, rest) => produce(new[] { first }.Concat(rest).ToList())
                );
            }

            return First(
                Sequence(
                    p,
                    List(
                        Sequence(
                            separator,
                            p,
                            (s, item) => item
                        ),
                        items => items
                    ),
                    (first, rest) => produce(new[] { first }.Concat(rest).ToList())
                ),
                Produce<TInput, TOutput>(() => produce(new List<TItem>()))
            );
        }

        /// <summary>
        /// Parse a sequence of productions and reduce them into a single output. If any item fails, rollback all and
        /// return failure
        /// </summary>
        /// <typeparam name="TInput, T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="produce"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Sequence<TInput, T1, T2, TOutput>(IParser<TInput, T1> p1, IParser<TInput, T2> p2, Func<T1, T2, TOutput> produce)
        {
            return new SequenceParser<TInput, TOutput>(
                new IParser<TInput>[] { p1, p2 },
                (list) => produce((T1)list[0], (T2)list[1]));
        }

        public static IParser<TInput, TOutput> Sequence<TInput, T1, T2, T3, TOutput>(IParser<TInput, T1> p1, IParser<TInput, T2> p2, IParser<TInput, T3> p3, Func<T1, T2, T3, TOutput> produce)
        {
            return new SequenceParser<TInput, TOutput>(
                new IParser<TInput>[] { p1, p2, p3 },
                (list) => produce((T1)list[0], (T2)list[1], (T3)list[2]));
        }

        public static IParser<TInput, TOutput> Sequence<TInput, T1, T2, T3, T4, TOutput>(IParser<TInput, T1> p1, IParser<TInput, T2> p2, IParser<TInput, T3> p3, IParser<TInput, T4> p4, Func<T1, T2, T3, T4, TOutput> produce)
        {
            return new SequenceParser<TInput, TOutput>(
                new IParser<TInput>[] { p1, p2, p3, p4 },
                (list) => produce((T1)list[0], (T2)list[1], (T3)list[2], (T4)list[3]));
        }

        public static IParser<TInput, TOutput> Sequence<TInput, T1, T2, T3, T4, T5, TOutput>(IParser<TInput, T1> p1, IParser<TInput, T2> p2, IParser<TInput, T3> p3, IParser<TInput, T4> p4, IParser<TInput, T5> p5, Func<T1, T2, T3, T4, T5, TOutput> produce)
        {
            return new SequenceParser<TInput, TOutput>(
                new IParser<TInput>[] { p1, p2, p3, p4, p5 },
                (list) => produce((T1)list[0], (T2)list[1], (T3)list[2], (T4)list[3], (T5)list[4]));
        }

        public static IParser<TInput, TOutput> Sequence<TInput, T1, T2, T3, T4, T5, T6, TOutput>(IParser<TInput, T1> p1, IParser<TInput, T2> p2, IParser<TInput, T3> p3, IParser<TInput, T4> p4, IParser<TInput, T5> p5, IParser<TInput, T6> p6, Func<T1, T2, T3, T4, T5, T6, TOutput> produce)
        {
            return new SequenceParser<TInput, TOutput>(
                new IParser<TInput>[] { p1, p2, p3, p4, p5, p6 },
                (list) => produce((T1)list[0], (T2)list[1], (T3)list[2], (T4)list[3], (T5)list[4], (T6)list[5]));
        }

        public static IParser<TInput, TOutput> Sequence<TInput, T1, T2, T3, T4, T5, T6, T7, TOutput>(IParser<TInput, T1> p1, IParser<TInput, T2> p2, IParser<TInput, T3> p3, IParser<TInput, T4> p4, IParser<TInput, T5> p5, IParser<TInput, T6> p6, IParser<TInput, T7> p7, Func<T1, T2, T3, T4, T5, T6, T7, TOutput> produce)
        {
            return new SequenceParser<TInput, TOutput>(
                new IParser<TInput>[] { p1, p2, p3, p4, p5, p6, p7 },
                (list) => produce((T1)list[0], (T2)list[1], (T3)list[2], (T4)list[3], (T5)list[4], (T6)list[5], (T7)list[6]));
        }

        public static IParser<TInput, TOutput> Sequence<TInput, T1, T2, T3, T4, T5, T6, T7, T8, TOutput>(IParser<TInput, T1> p1, IParser<TInput, T2> p2, IParser<TInput, T3> p3, IParser<TInput, T4> p4, IParser<TInput, T5> p5, IParser<TInput, T6> p6, IParser<TInput, T7> p7, IParser<TInput, T8> p8, Func<T1, T2, T3, T4, T5, T6, T7, T8, TOutput> produce)
        {
            return new SequenceParser<TInput, TOutput>(
                new IParser<TInput>[] { p1, p2, p3, p4, p5, p6, p7, p8 },
                (list) => produce((T1)list[0], (T2)list[1], (T3)list[2], (T4)list[3], (T5)list[4], (T6)list[5], (T7)list[6], (T8)list[7]));
        }

        public static IParser<TInput, TOutput> Sequence<TInput, T1, T2, T3, T4, T5, T6, T7, T8, T9, TOutput>(IParser<TInput, T1> p1, IParser<TInput, T2> p2, IParser<TInput, T3> p3, IParser<TInput, T4> p4, IParser<TInput, T5> p5, IParser<TInput, T6> p6, IParser<TInput, T7> p7, IParser<TInput, T8> p8, IParser<TInput, T9> p9, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOutput> produce)
        {
            return new SequenceParser<TInput, TOutput>(
                new IParser<TInput>[] { p1, p2, p3, p4, p5, p6, p7, p8, p9 },
                (list) => produce((T1)list[0], (T2)list[1], (T3)list[2], (T4)list[3], (T5)list[4], (T6)list[5], (T7)list[6], (T8)list[7], (T9)list[8]));
        }

        /// <summary>
        /// Transform one node into another node to fit into the grammar
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="TMiddle"></typeparam>
        /// <param name="parser"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static IParser<TInput, TOutput> Transform<TInput, TMiddle, TOutput>(IParser<TInput, TMiddle> parser, Func<TMiddle, TOutput> transform)
        {
            return new TransformParser<TInput, TMiddle, TOutput>(parser, transform);
        }
    }
}
