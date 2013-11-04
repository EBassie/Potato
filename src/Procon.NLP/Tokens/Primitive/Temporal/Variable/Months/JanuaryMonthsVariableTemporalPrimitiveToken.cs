﻿using System;

namespace Procon.Nlp.Tokens.Primitive.Temporal.Variable.Months {
    public class JanuaryMonthsVariableTemporalPrimitiveToken : MonthMonthsVariableTemporalPrimitiveToken {

        public static Phrase Parse(IStateNlp state, Phrase phrase) {
            return TokenReflection.CreateDescendants<JanuaryMonthsVariableTemporalPrimitiveToken>(state, phrase);
        }

        public JanuaryMonthsVariableTemporalPrimitiveToken() {
            this.Pattern = new DateTimePatternNlp() {
                Rule = TimeType.Definitive,
                Year = DateTime.Now.Month <= 1 ? DateTime.Now.Year : DateTime.Now.Year + 1,
                Month = 1
            };
        }
    }
}
