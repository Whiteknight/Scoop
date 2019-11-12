﻿using System;
using System.Collections.Generic;

namespace Scoop.Parsing.Parsers
{
    public static class ParserExtensions
    {
        public static IParser<TInput, TOutput> List<TInput, TItem, TOutput>(this IParser<TInput, TItem> p, Func<IReadOnlyList<TItem>, TOutput> produce, bool atLeastOne = false)
        {
            return new ListParser<TInput, TItem, TOutput>(p, produce, atLeastOne);
        }

        public static IParser<TInput, TOutput> Optional<TInput, TOutput>(this IParser<TInput, TOutput> p, Func<TOutput> getDefault = null)
        {
            return new OptionalParser<TInput, TOutput>(p, getDefault);
        }

        public static IParser<TInput, TOutput> Or<TInput, TOutput>(this IParser<TInput, TOutput> p, Func<ISequence<TInput>, TOutput> produce)
        {
            return new RequiredParser<TInput, TOutput>(p, produce);
        }

        public static IParser<TInput, TOutput> Transform<TInput, TMiddle, TOutput>(this IParser<TInput, TMiddle> parser, Func<TMiddle, TOutput> transform)
        {
            return new TransformParser<TInput, TMiddle, TOutput>(parser, transform);
        }
    }
}
