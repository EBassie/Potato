﻿namespace Procon.Fuzzy.Tokens.Operator.Logical {
    public class OrLogicalOperatorToken : LogicalOperatorToken {
        public static Phrase Parse(IFuzzyState state, Phrase phrase) {
            return TokenReflection.CreateDescendants<OrLogicalOperatorToken>(state, phrase);
        }
    }
}