﻿namespace Procon.Fuzzy.Tokens.Operator.Logical.Equality {
    public class LessThanEqualToEqualityLogicalOperatorToken : EqualityLogicalOperatorToken {
        public static Phrase Parse(IFuzzyState state, Phrase phrase) {
            return TokenReflection.CreateDescendants<LessThanEqualToEqualityLogicalOperatorToken>(state, phrase);
        }
    }
}