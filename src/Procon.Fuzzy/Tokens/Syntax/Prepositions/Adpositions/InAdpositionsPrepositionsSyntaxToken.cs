﻿
namespace Procon.Fuzzy.Tokens.Syntax.Prepositions.Adpositions {
    public class InAdpositionsPrepositionsSyntaxToken : AdpositionsPrepositionsSyntaxToken {
        public static Phrase Parse(IFuzzyState state, Phrase phrase) {
            return TokenReflection.CreateDescendants<InAdpositionsPrepositionsSyntaxToken>(state, phrase);
        }
    }
}