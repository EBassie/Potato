﻿using System;
using System.Collections.Generic;
using Procon.Net.Protocols.Myrcon.Frostbite.Objects;

namespace Procon.Net.Protocols.Myrcon.Frostbite.Battlefield.Battlefield3.Objects {
    [Serializable]
    public class Battlefield3Players : FrostbitePlayers {

        public new Battlefield3Players Parse(List<string> words) {

            int currentOffset = 0;
            int parameterCount = 0;

            //if (words.Count > currentOffset && int.TryParse(words[currentOffset++], out playerCount) == true) {

            //    if (words.Count > 0 && int.TryParse(words[currentOffset++], out parameterCount) == true) {
            //        List<string> lstParameters = words.GetRange(currentOffset, parameterCount);

            //        currentOffset += parameterCount;

            //        for (int i = 0; i < playerCount; i++) {
            //            if (words.Count > currentOffset + (i * parameterCount)) {
            //                this.Add(new FrostbitePlayer(lstParameters, words.GetRange(currentOffset + i * parameterCount, parameterCount)));
            //            }
            //        }

            //    }

            //}

            if (words.Count > 0 && int.TryParse(words[currentOffset++], out parameterCount) == true) {
                List<string> lstParameters = words.GetRange(currentOffset, parameterCount);
                currentOffset += parameterCount;

                int playerCount = 0;
                if (words.Count > currentOffset && int.TryParse(words[currentOffset++], out playerCount) == true) {
                    for (int i = 0; i < playerCount; i++) {
                        if (words.Count > currentOffset + (i * parameterCount)) {
                            this.Add(FrostbitePlayer.Parse(lstParameters, words.GetRange(currentOffset + i * parameterCount, parameterCount)));
                        }
                    }
                }
            }
            return this;
        }
    }
}