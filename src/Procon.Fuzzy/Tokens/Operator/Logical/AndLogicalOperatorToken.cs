﻿namespace Procon.Fuzzy.Tokens.Operator.Logical {
    public class AndLogicalOperatorToken : LogicalOperatorToken {
        public static Phrase Parse(IFuzzyState state, Phrase phrase) {
            return TokenReflection.CreateDescendants<AndLogicalOperatorToken>(state, phrase);
        }
    }
}