﻿namespace Procon.Fuzzy.Tokens.Syntax.Adjectives {
    public class EveryAdjectiveSyntaxToken : AdjectiveSyntaxToken {
        public static Phrase Parse(IFuzzyState state, Phrase phrase) {
            return TokenReflection.CreateDescendants<EveryAdjectiveSyntaxToken>(state, phrase);
        }
    }
}