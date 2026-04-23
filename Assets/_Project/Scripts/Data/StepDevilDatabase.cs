namespace StepDevil
{
    public static class StepDevilDatabase
    {
        public const int LevelCount = 65;

        static StepDevilStoneDef H(StepDevilStoneType t) => StepDevilStoneDef.Honest(t);
        static StepDevilStoneDef S(StepDevilStoneType t, string c, string l, string i) =>
            StepDevilStoneDef.WithOverrides(t, c, l, i);

        // ── Unicode icon constants ──────────────────────────────────────────
        const string OK  = "\u2713"; // ✓  true icon for Solid
        const string NO  = "\u2717"; // ✗  true icon for Void
        const string STR = "\u2605"; // ★  true icon for Bonus
        const string UP  = "\u2191"; // ↑  true icon for Spring
        const string REV = "\u21BB"; // ↻  true icon for Mirror

        // ── Stone type aliases ─────────────────────────────────────────────
        const StepDevilStoneType Sol = StepDevilStoneType.Solid;
        const StepDevilStoneType Voi = StepDevilStoneType.Void;
        const StepDevilStoneType Bon = StepDevilStoneType.Bonus;
        const StepDevilStoneType Spr = StepDevilStoneType.Spring;
        const StepDevilStoneType Mir = StepDevilStoneType.Mirror;

        public static readonly StepDevilWorldDef[] Worlds =
        {
            // 0 – Training Ground
            new StepDevilWorldDef
            {
                Number  = 1, Name = "Training Ground", Icon = "\U0001F331",
                Hint    = "All signals are honest \u2014 trust what you see!",
                RuleHtml = "Welcome to the Training Ground!\n\n" +
                           "All signals here are <b>completely honest</b>.\n\n" +
                           "Learn what each stone does \u2014 this knowledge will save you later."
            },
            // 1 – The Colour Trick
            new StepDevilWorldDef
            {
                Number  = 2, Name = "The Colour Trick", Icon = "\U0001F3A8",
                Hint    = "COLOURS may lie \u2014 trust the LABEL & ICON!",
                RuleHtml = "The devil has started <b>lying with colours</b>!\n\n" +
                           "A red stone might be safe. A green stone might be deadly.\n\n" +
                           "<b>Ignore the colour</b> \u2014 read the label and icon instead."
            },
            // 2 – Words Are Weapons
            new StepDevilWorldDef
            {
                Number  = 3, Name = "Words Are Weapons", Icon = "\U0001F4DD",
                Hint    = "LABELS may lie \u2014 trust the COLOUR & ICON!",
                RuleHtml = "Now the devil twists <b>words too</b>!\n\n" +
                           "A stone labelled DANGER might be your only safe path.\n\n" +
                           "Trust the colour and icon \u2014 the label is now the enemy."
            },
            // 3 – The Icon Inverts
            new StepDevilWorldDef
            {
                Number  = 4, Name = "The Icon Inverts", Icon = "\U0001F500",
                Hint    = "All three signals can lie \u2014 find two that agree!",
                RuleHtml = "The devil has learned to <b>lie with icons</b>.\n\n" +
                           "Colour, label <i>and</i> icon can each lie independently.\n\n" +
                           "Two signals that agree \u2014 that is the truth."
            },
            // 4 – The Devil's Garden
            new StepDevilWorldDef
            {
                Number  = 5, Name = "The Devil's Garden", Icon = "\U0001F608",
                Hint    = "Two signals may lie at once \u2014 find the honest one!",
                RuleHtml = "Welcome to <b>The Devil's Garden</b>.\n\n" +
                           "Two out of three signals may lie simultaneously.\n\n" +
                           "Only one signal tells the truth \u2014 find it fast or fall."
            },
            // 5 – Abyss
            new StepDevilWorldDef
            {
                Number  = 6, Name = "Abyss", Icon = "\U0001F311",
                Hint    = "ALL signals may lie. Trust nothing. Feel the pattern.",
                RuleHtml = "You have entered the <b>Abyss</b>.\n\n" +
                           "All three signals may lie at once.\n\n" +
                           "Only pattern-reading and raw instinct can save you now."
            }
        };

        public static readonly StepDevilLevelDef[] Levels =
        {
            // ════════════════════════════════════════════════════════════
            // WORLD 1 – Training Ground  (Levels 1-5)
            // All signals honest. Introduce all stone types gradually.
            // ════════════════════════════════════════════════════════════

            new StepDevilLevelDef(1, 0, "Baby Steps", new[]
            {
                new[] { H(Sol), H(Voi) },
                new[] { H(Voi), H(Sol) },
                new[] { H(Sol), H(Voi) }
            }),
            new StepDevilLevelDef(2, 0, "First Light", new[]
            {
                new[] { H(Voi), H(Sol) },
                new[] { H(Sol), H(Voi) },
                new[] { H(Voi), H(Sol) },
                new[] { H(Sol), H(Voi) }
            }),
            new StepDevilLevelDef(3, 0, "Golden Path", new[]
            {
                new[] { H(Bon), H(Voi) },
                new[] { H(Sol), H(Voi) },
                new[] { H(Voi), H(Bon) },
                new[] { H(Sol), H(Voi) }
            }),
            new StepDevilLevelDef(4, 0, "Spring Forward", new[]
            {
                new[] { H(Spr), H(Voi) },
                new[] { H(Sol), H(Voi) },
                new[] { H(Voi), H(Spr) },
                new[] { H(Bon), H(Voi) },
                new[] { H(Voi), H(Sol) }
            }),
            new StepDevilLevelDef(5, 0, "Graduation Day", new[]
            {
                new[] { H(Sol), H(Voi) },
                new[] { H(Spr), H(Voi) },
                new[] { H(Voi), H(Bon) },
                new[] { H(Mir), H(Voi) },
                new[] { H(Sol), H(Voi) }
            }),

            // ════════════════════════════════════════════════════════════
            // WORLD 2 – The Colour Trick  (Levels 6-15)
            // Colours lie. Labels and icons are always honest.
            // Safe stone formula: S(Sol, WRONG_COLOR, "SAFE", OK)
            // Void  stone formula: S(Voi, WRONG_COLOR, "DANGER", NO)
            // ════════════════════════════════════════════════════════════

            new StepDevilLevelDef(6, 1, "Red Means Safe?", new[]
            {
                new[] { S(Sol,"red","SAFE",OK),    S(Voi,"green","DANGER",NO) },
                new[] { S(Voi,"green","DANGER",NO),S(Sol,"red","SAFE",OK)    },
                new[] { S(Sol,"red","SAFE",OK),    S(Voi,"green","DANGER",NO) },
                new[] { H(Voi),                    S(Sol,"red","SAFE",OK)    }
            }),
            new StepDevilLevelDef(7, 1, "Green Is a Liar", new[]
            {
                new[] { S(Voi,"green","DANGER",NO),S(Sol,"red","SAFE",OK)    },
                new[] { S(Sol,"red","SAFE",OK),    S(Voi,"green","DANGER",NO) },
                new[] { S(Voi,"green","DANGER",NO),H(Sol)                    },
                new[] { S(Sol,"red","SAFE",OK),    S(Voi,"green","DANGER",NO) }
            }),
            new StepDevilLevelDef(8, 1, "Colour Blind", new[]
            {
                new[] { S(Sol,"red","SAFE",OK),    S(Voi,"red","DANGER",NO)   },
                new[] { S(Voi,"green","DANGER",NO),S(Sol,"green","SAFE",OK)  },
                new[] { S(Sol,"red","SAFE",OK),    S(Voi,"green","DANGER",NO) },
                new[] { H(Sol),                    S(Voi,"green","DANGER",NO) }
            }),
            new StepDevilLevelDef(9, 1, "Painted Lies", new[]
            {
                new[] { S(Voi,"green","DANGER",NO),S(Sol,"red","SAFE",OK)    },
                new[] { S(Sol,"green","SAFE",OK),  S(Voi,"red","DANGER",NO)  },
                new[] { S(Voi,"red","DANGER",NO),  S(Sol,"green","SAFE",OK)  },
                new[] { S(Sol,"red","SAFE",OK),    S(Voi,"green","DANGER",NO) }
            }),
            new StepDevilLevelDef(10, 1, "Deep Blue Lie", new[]
            {
                new[] { S(Sol,"blue","SAFE",OK),   S(Voi,"green","DANGER",NO) },
                new[] { S(Voi,"blue","DANGER",NO), S(Sol,"red","SAFE",OK)    },
                new[] { S(Sol,"blue","SAFE",OK),   S(Voi,"red","DANGER",NO)  },
                new[] { S(Bon,"blue","BONUS",STR), S(Voi,"green","DANGER",NO) },
                new[] { S(Voi,"blue","DANGER",NO), S(Sol,"green","SAFE",OK)  }
            }),
            new StepDevilLevelDef(11, 1, "Grey Area", new[]
            {
                new[] { S(Sol,"grey","SAFE",OK),   S(Voi,"grey","DANGER",NO) },
                new[] { S(Voi,"grey","DANGER",NO), S(Sol,"red","SAFE",OK)    },
                new[] { S(Mir,"grey","MIRROR",REV),S(Voi,"green","DANGER",NO)},
                new[] { S(Bon,"grey","BONUS",STR), S(Voi,"grey","DANGER",NO) },
                new[] { S(Voi,"grey","DANGER",NO), S(Sol,"grey","SAFE",OK)   }
            }),
            new StepDevilLevelDef(12, 1, "Rainbow Road", new[]
            {
                new[] { S(Voi,"yellow","DANGER",NO),S(Sol,"blue","SAFE",OK),   S(Voi,"green","DANGER",NO) },
                new[] { S(Sol,"gold","SAFE",OK),    S(Voi,"red","DANGER",NO)  },
                new[] { S(Voi,"blue","DANGER",NO),  S(Bon,"yellow","BONUS",STR),S(Voi,"grey","DANGER",NO) },
                new[] { S(Spr,"red","BOUNCE",UP),   S(Voi,"gold","DANGER",NO) },
                new[] { S(Voi,"green","DANGER",NO), S(Sol,"red","SAFE",OK),    S(Voi,"blue","DANGER",NO)  }
            }),
            new StepDevilLevelDef(13, 1, "Colour Storm", new[]
            {
                new[] { S(Voi,"blue","DANGER",NO),  S(Sol,"yellow","SAFE",OK),  S(Voi,"grey","DANGER",NO)  },
                new[] { S(Spr,"blue","BOUNCE",UP),  S(Voi,"yellow","DANGER",NO),S(Voi,"gold","DANGER",NO)  },
                new[] { S(Voi,"red","DANGER",NO),   S(Mir,"blue","MIRROR",REV), S(Voi,"yellow","DANGER",NO)},
                new[] { S(Sol,"grey","SAFE",OK),    S(Voi,"blue","DANGER",NO),  S(Voi,"gold","DANGER",NO)  },
                new[] { S(Bon,"red","BONUS",STR),   S(Voi,"grey","DANGER",NO),  S(Voi,"blue","DANGER",NO)  }
            }),
            new StepDevilLevelDef(14, 1, "Dye Hard", new[]
            {
                new[] { S(Sol,"gold","SAFE",OK),    S(Voi,"blue","DANGER",NO),  S(Voi,"grey","DANGER",NO)  },
                new[] { S(Voi,"yellow","DANGER",NO),S(Mir,"red","MIRROR",REV),  S(Voi,"gold","DANGER",NO)  },
                new[] { S(Spr,"blue","BOUNCE",UP),  S(Voi,"gold","DANGER",NO),  S(Voi,"yellow","DANGER",NO)},
                new[] { S(Voi,"grey","DANGER",NO),  S(Sol,"yellow","SAFE",OK),  S(Voi,"red","DANGER",NO)   },
                new[] { S(Bon,"grey","BONUS",STR),  S(Voi,"blue","DANGER",NO),  S(Voi,"gold","DANGER",NO)  },
                new[] { S(Sol,"red","SAFE",OK),     S(Voi,"yellow","DANGER",NO),S(Voi,"grey","DANGER",NO)  }
            }),
            new StepDevilLevelDef(15, 1, "Boss: Colour King", new[]
            {
                new[] { S(Voi,"gold","DANGER",NO),  S(Sol,"blue","SAFE",OK),    S(Voi,"grey","DANGER",NO)  },
                new[] { S(Mir,"gold","MIRROR",REV), S(Voi,"yellow","DANGER",NO),S(Voi,"red","DANGER",NO)   },
                new[] { S(Voi,"blue","DANGER",NO),  S(Spr,"red","BOUNCE",UP),   S(Voi,"gold","DANGER",NO)  },
                new[] { S(Sol,"yellow","SAFE",OK),  S(Voi,"grey","DANGER",NO),  S(Voi,"blue","DANGER",NO)  },
                new[] { S(Voi,"red","DANGER",NO),   S(Mir,"grey","MIRROR",REV), S(Voi,"gold","DANGER",NO)  },
                new[] { S(Bon,"yellow","BONUS",STR),S(Voi,"blue","DANGER",NO),  S(Voi,"grey","DANGER",NO)  },
                new[] { S(Voi,"gold","DANGER",NO),  S(Sol,"red","SAFE",OK),     S(Voi,"blue","DANGER",NO)  }
            }),

            // ════════════════════════════════════════════════════════════
            // WORLD 3 – Words Are Weapons  (Levels 16-30)
            // Labels CAN lie. Colour also still lies (stacked from W2).
            // Icon remains honest throughout this world.
            // Read the ICON: OK=safe, NO=void, STR=bonus, UP=spring, REV=mirror
            // ════════════════════════════════════════════════════════════

            // Lie guide for this world:
            //  Solid lied label: S(Sol, anyColor, "DANGER", OK)
            //  Void  lied label: S(Voi, anyColor, "SAFE",   NO)
            //  Both col+label lie (icon honest is the only truth):
            //    S(Sol, "red",   "DANGER", OK)
            //    S(Voi, "green", "SAFE",   NO)

            new StepDevilLevelDef(16, 2, "Word Games", new[]
            {
                new[] { S(Sol,"green","DANGER",OK), S(Voi,"red","SAFE",NO)    },
                new[] { S(Voi,"red","SAFE",NO),    S(Sol,"green","DANGER",OK) },
                new[] { S(Sol,"red","DANGER",OK),  S(Voi,"green","SAFE",NO)   },
                new[] { H(Bon),                    S(Voi,"red","SAFE",NO)     }
            }),
            new StepDevilLevelDef(17, 2, "False Words", new[]
            {
                new[] { S(Sol,"red","DANGER",OK),  S(Voi,"green","SAFE",NO)   },
                new[] { S(Voi,"green","SAFE",NO),  S(Sol,"green","DANGER",OK) },
                new[] { S(Sol,"red","DANGER",OK),  S(Voi,"red","SAFE",NO)    },
                new[] { S(Bon,"red","DANGER",STR), S(Voi,"green","SAFE",NO)   }
            }),
            new StepDevilLevelDef(18, 2, "Doublespeak", new[]
            {
                new[] { H(Sol),                    S(Voi,"green","SAFE",NO)   },
                new[] { S(Sol,"red","DANGER",OK),  H(Voi)                    },
                new[] { S(Voi,"green","SAFE",NO),  S(Sol,"red","DANGER",OK)  },
                new[] { H(Sol),                    S(Voi,"red","SAFE",NO)    },
                new[] { S(Sol,"green","DANGER",OK),H(Voi)                    }
            }),
            new StepDevilLevelDef(19, 2, "Label Trap", new[]
            {
                new[] { S(Sol,"blue","DANGER",OK),  S(Voi,"blue","SAFE",NO)   },
                new[] { S(Voi,"grey","SAFE",NO),    S(Sol,"grey","DANGER",OK) },
                new[] { S(Sol,"red","DANGER",OK),   S(Voi,"green","SAFE",NO)  },
                new[] { S(Bon,"blue","DANGER",STR), S(Voi,"grey","SAFE",NO)   },
                new[] { S(Sol,"grey","DANGER",OK),  S(Voi,"blue","SAFE",NO)   }
            }),
            new StepDevilLevelDef(20, 2, "Twisted Signs", new[]
            {
                new[] { S(Sol,"red","SAFE",OK),     S(Voi,"green","DANGER",NO),S(Voi,"blue","SAFE",NO)  },
                new[] { S(Sol,"green","DANGER",OK), S(Voi,"red","SAFE",NO)   },
                new[] { S(Voi,"blue","SAFE",NO),    S(Sol,"blue","DANGER",OK), S(Voi,"grey","DANGER",NO) },
                new[] { S(Spr,"green","DANGER",UP), S(Voi,"red","SAFE",NO)   },
                new[] { S(Sol,"grey","DANGER",OK),  S(Voi,"grey","SAFE",NO),   S(Voi,"blue","SAFE",NO)  }
            }),
            new StepDevilLevelDef(21, 2, "Mirror Words", new[]
            {
                new[] { S(Sol,"red","DANGER",OK),   S(Voi,"green","SAFE",NO)  },
                new[] { S(Mir,"grey","SAFE",REV),   S(Voi,"red","DANGER",NO)  },
                new[] { S(Sol,"blue","DANGER",OK),  S(Voi,"blue","SAFE",NO)   },
                new[] { S(Sol,"red","DANGER",OK),   S(Voi,"green","SAFE",NO)  },
                new[] { S(Bon,"red","SAFE",STR),    S(Voi,"green","DANGER",NO) }
            }),
            new StepDevilLevelDef(22, 2, "Reverse Logic", new[]
            {
                new[] { S(Sol,"red","DANGER",OK),   S(Voi,"red","SAFE",NO)    },
                new[] { S(Voi,"green","DANGER",NO), S(Sol,"green","SAFE",OK)  },
                new[] { S(Sol,"blue","DANGER",OK),  S(Voi,"blue","SAFE",NO)   },
                new[] { S(Spr,"red","DANGER",UP),   S(Voi,"green","SAFE",NO)  },
                new[] { S(Sol,"grey","DANGER",OK),  S(Voi,"grey","SAFE",NO),   S(Voi,"blue","SAFE",NO)  }
            }),
            new StepDevilLevelDef(23, 2, "Colour+Word", new[]
            {
                new[] { S(Sol,"red","DANGER",OK),   S(Voi,"green","SAFE",NO)  },
                new[] { S(Voi,"green","SAFE",NO),   S(Sol,"red","DANGER",OK)  },
                new[] { S(Sol,"blue","DANGER",OK),  S(Voi,"gold","SAFE",NO)   },
                new[] { S(Bon,"red","DANGER",STR),  S(Voi,"green","SAFE",NO)  },
                new[] { S(Sol,"grey","DANGER",OK),  S(Voi,"blue","SAFE",NO)   }
            }),
            new StepDevilLevelDef(24, 2, "Signal Noise", new[]
            {
                new[] { S(Sol,"red","DANGER",OK),   S(Voi,"green","SAFE",NO),  S(Voi,"blue","DANGER",NO) },
                new[] { S(Sol,"gold","SAFE",OK),    S(Voi,"gold","DANGER",NO) },
                new[] { S(Mir,"red","DANGER",REV),  S(Voi,"green","SAFE",NO),  S(Voi,"grey","SAFE",NO)   },
                new[] { S(Sol,"blue","DANGER",OK),  S(Voi,"blue","SAFE",NO),   S(Voi,"red","SAFE",NO)    },
                new[] { S(Bon,"grey","DANGER",STR), S(Voi,"gold","SAFE",NO),   S(Voi,"green","SAFE",NO)  }
            }),
            new StepDevilLevelDef(25, 2, "Deception Maze", new[]
            {
                new[] { S(Sol,"red","DANGER",OK),   S(Voi,"green","SAFE",NO),  S(Voi,"red","DANGER",NO)  },
                new[] { S(Voi,"blue","SAFE",NO),    S(Sol,"blue","DANGER",OK)  },
                new[] { S(Sol,"grey","DANGER",OK),  S(Voi,"grey","SAFE",NO),   S(Voi,"gold","SAFE",NO)   },
                new[] { S(Spr,"red","DANGER",UP),   S(Voi,"green","SAFE",NO),  S(Voi,"blue","SAFE",NO)   },
                new[] { S(Sol,"gold","DANGER",OK),  S(Voi,"gold","SAFE",NO),   S(Voi,"red","SAFE",NO)    },
                new[] { S(Bon,"blue","DANGER",STR), S(Voi,"blue","SAFE",NO),   S(Voi,"grey","DANGER",NO) }
            }),
            new StepDevilLevelDef(26, 2, "Word Fog", new[]
            {
                new[] { S(Sol,"red","DANGER",OK),   S(Voi,"green","SAFE",NO)  },
                new[] { S(Sol,"green","DANGER",OK), S(Voi,"red","SAFE",NO),    S(Voi,"blue","SAFE",NO)   },
                new[] { S(Mir,"grey","SAFE",REV),   S(Voi,"grey","DANGER",NO) },
                new[] { S(Sol,"blue","DANGER",OK),  S(Voi,"gold","SAFE",NO),   S(Voi,"grey","SAFE",NO)   },
                new[] { S(Sol,"red","DANGER",OK),   S(Voi,"blue","SAFE",NO),   S(Voi,"green","SAFE",NO)  }
            }),
            new StepDevilLevelDef(27, 2, "Mixed Signals", new[]
            {
                new[] { S(Sol,"red","DANGER",OK),   S(Voi,"green","SAFE",NO),  S(Voi,"red","DANGER",NO)  },
                new[] { S(Sol,"gold","DANGER",OK),  S(Voi,"gold","SAFE",NO)   },
                new[] { S(Voi,"blue","SAFE",NO),    S(Sol,"blue","DANGER",OK),  S(Voi,"yellow","SAFE",NO) },
                new[] { S(Bon,"red","DANGER",STR),  S(Voi,"red","SAFE",NO)    },
                new[] { S(Sol,"grey","DANGER",OK),  S(Voi,"grey","SAFE",NO),   S(Voi,"gold","SAFE",NO)   },
                new[] { S(Spr,"blue","DANGER",UP),  S(Voi,"green","SAFE",NO),  S(Voi,"red","SAFE",NO)    }
            }),
            new StepDevilLevelDef(28, 2, "Devil's Tongue", new[]
            {
                new[] { S(Sol,"red","DANGER",OK),   S(Voi,"green","SAFE",NO),  S(Voi,"blue","SAFE",NO)   },
                new[] { S(Sol,"blue","DANGER",OK),  S(Voi,"blue","SAFE",NO),   S(Voi,"grey","SAFE",NO)   },
                new[] { S(Mir,"gold","SAFE",REV),   S(Voi,"red","DANGER",NO),  S(Voi,"green","DANGER",NO)},
                new[] { S(Sol,"grey","DANGER",OK),  S(Voi,"gold","SAFE",NO),   S(Voi,"red","SAFE",NO)    },
                new[] { S(Bon,"red","DANGER",STR),  S(Voi,"green","SAFE",NO),  S(Voi,"grey","SAFE",NO)   },
                new[] { S(Sol,"gold","DANGER",OK),  S(Voi,"grey","SAFE",NO),   S(Voi,"blue","SAFE",NO)   },
                new[] { S(Sol,"red","DANGER",OK),   S(Voi,"blue","SAFE",NO),   S(Voi,"gold","SAFE",NO)   }
            }),
            new StepDevilLevelDef(29, 2, "Lie Cascade", new[]
            {
                new[] { S(Sol,"red","DANGER",OK),   S(Voi,"green","SAFE",NO),  S(Voi,"red","DANGER",NO)  },
                new[] { S(Voi,"blue","SAFE",NO),    S(Sol,"blue","DANGER",OK),  S(Voi,"grey","SAFE",NO)   },
                new[] { S(Spr,"red","DANGER",UP),   S(Voi,"green","SAFE",NO),  S(Voi,"gold","SAFE",NO)   },
                new[] { S(Sol,"grey","DANGER",OK),  S(Voi,"gold","SAFE",NO),   S(Voi,"blue","SAFE",NO)   },
                new[] { S(Sol,"gold","DANGER",OK),  S(Voi,"red","SAFE",NO),    S(Voi,"grey","SAFE",NO)   },
                new[] { S(Bon,"blue","DANGER",STR), S(Voi,"grey","SAFE",NO),   S(Voi,"gold","SAFE",NO)   }
            }),
            new StepDevilLevelDef(30, 2, "Boss: Word Lord", new[]
            {
                new[] { S(Sol,"red","DANGER",OK),   S(Voi,"green","SAFE",NO),  S(Voi,"blue","SAFE",NO)   },
                new[] { S(Mir,"red","SAFE",REV),    S(Voi,"green","DANGER",NO),S(Voi,"grey","DANGER",NO) },
                new[] { S(Sol,"blue","DANGER",OK),  S(Voi,"blue","SAFE",NO),   S(Voi,"red","SAFE",NO)    },
                new[] { S(Spr,"gold","DANGER",UP),  S(Voi,"red","SAFE",NO),    S(Voi,"green","SAFE",NO)  },
                new[] { S(Sol,"grey","DANGER",OK),  S(Voi,"grey","SAFE",NO),   S(Voi,"gold","SAFE",NO)   },
                new[] { S(Bon,"blue","DANGER",STR), S(Voi,"blue","SAFE",NO),   S(Voi,"grey","DANGER",NO) },
                new[] { S(Sol,"gold","DANGER",OK),  S(Voi,"red","SAFE",NO),    S(Voi,"blue","SAFE",NO)   }
            }),

            // ════════════════════════════════════════════════════════════
            // WORLD 4 – The Icon Inverts  (Levels 31-42)
            // Icons can now lie independently. All three signals deceptive.
            // Strategy: find two signals that agree = truth.
            // Lie legend (what signal is lying):
            //   col only:        S(Sol,"red","SAFE",OK)       S(Voi,"green","DANGER",NO)
            //   label only:      S(Sol,"green","DANGER",OK)   S(Voi,"red","SAFE",NO)
            //   icon only:       S(Sol,"green","SAFE",NO)     S(Voi,"red","DANGER",OK)
            //   col+label:       S(Sol,"red","DANGER",OK)     S(Voi,"green","SAFE",NO)
            //   col+icon:        S(Sol,"red","SAFE",NO)       S(Voi,"green","DANGER",OK)
            //   label+icon:      S(Sol,"green","DANGER",NO)   S(Voi,"red","SAFE",OK)
            // ════════════════════════════════════════════════════════════

            new StepDevilLevelDef(31, 3, "False Marks", new[]
            {
                // icon lies: Solid shows ✗, Void shows ✓
                new[] { S(Sol,"green","SAFE",NO),  S(Voi,"red","DANGER",OK)   },
                new[] { S(Sol,"green","SAFE",NO),  S(Voi,"red","DANGER",OK)   },
                new[] { S(Voi,"red","DANGER",OK),  S(Sol,"green","SAFE",NO)   },
                new[] { H(Sol),                    S(Voi,"red","DANGER",OK)   }
            }),
            new StepDevilLevelDef(32, 3, "Three Liars", new[]
            {
                new[] { S(Sol,"red","SAFE",OK),    S(Voi,"green","DANGER",NO)  }, // colour lies
                new[] { S(Sol,"green","DANGER",OK),S(Voi,"red","SAFE",NO)      }, // label lies
                new[] { S(Sol,"green","SAFE",NO),  S(Voi,"red","DANGER",OK)    }, // icon lies
                new[] { H(Sol),                    H(Voi)                       }
            }),
            new StepDevilLevelDef(33, 3, "Icon Trap", new[]
            {
                new[] { S(Sol,"green","SAFE",NO),  S(Voi,"red","DANGER",OK)   },
                new[] { S(Bon,"gold","BONUS",NO),  S(Voi,"red","DANGER",STR)  }, // Bonus with icon lie
                new[] { S(Sol,"red","DANGER",OK),  S(Voi,"green","SAFE",NO)   }, // col+label lie
                new[] { S(Sol,"green","SAFE",NO),  S(Voi,"red","DANGER",OK)   },
                new[] { H(Sol),                    S(Voi,"green","DANGER",OK)  }  // Void icon lies
            }),
            new StepDevilLevelDef(34, 3, "Symbol War", new[]
            {
                new[] { S(Sol,"red","SAFE",NO),    S(Voi,"green","DANGER",OK)  }, // col+icon lie
                new[] { S(Sol,"green","DANGER",NO),S(Voi,"red","SAFE",OK)      }, // label+icon lie
                new[] { S(Sol,"red","SAFE",OK),    S(Voi,"green","DANGER",NO)  }, // colour lies only
                new[] { S(Spr,"green","BOUNCE",NO),S(Voi,"red","DANGER",OK)    }, // icon lies
                new[] { H(Sol),                    H(Voi)                       }
            }),
            new StepDevilLevelDef(35, 3, "Spinning Marks", new[]
            {
                new[] { S(Sol,"green","SAFE",NO),  S(Voi,"red","DANGER",OK),   S(Voi,"blue","SAFE",OK)   },
                new[] { S(Sol,"red","DANGER",OK),  S(Voi,"green","SAFE",NO)    },
                new[] { S(Mir,"purple","MIRROR",NO),S(Voi,"red","DANGER",REV), S(Voi,"green","SAFE",REV) },
                new[] { S(Sol,"green","SAFE",NO),  S(Voi,"red","DANGER",OK),   S(Voi,"grey","SAFE",OK)   },
                new[] { S(Bon,"gold","BONUS",NO),  S(Voi,"red","DANGER",STR),  S(Voi,"blue","SAFE",STR)  }
            }),
            new StepDevilLevelDef(36, 3, "Consensus", new[]
            {
                new[] { S(Sol,"red","SAFE",OK),    S(Voi,"red","DANGER",NO)    }, // label+icon agree (sol)
                new[] { S(Sol,"green","DANGER",OK),S(Voi,"red","SAFE",NO)      }, // label lies on both
                new[] { S(Sol,"green","SAFE",NO),  S(Voi,"red","DANGER",OK)    }, // icon lies on both
                new[] { S(Sol,"red","SAFE",OK),    S(Voi,"green","SAFE",NO),   S(Voi,"blue","DANGER",OK)  },
                new[] { S(Sol,"blue","SAFE",OK),   S(Voi,"blue","DANGER",NO)   }
            }),
            new StepDevilLevelDef(37, 3, "Icon Storm", new[]
            {
                new[] { S(Sol,"red","SAFE",NO),    S(Voi,"green","DANGER",OK), S(Voi,"blue","SAFE",OK)   },
                new[] { S(Sol,"green","DANGER",OK),S(Voi,"green","SAFE",NO)   },
                new[] { S(Spr,"red","BOUNCE",NO),  S(Voi,"green","DANGER",OK), S(Voi,"grey","SAFE",OK)   },
                new[] { S(Sol,"blue","SAFE",NO),   S(Voi,"blue","DANGER",OK)  },
                new[] { S(Sol,"grey","DANGER",NO), S(Voi,"red","SAFE",OK),    S(Voi,"gold","SAFE",OK)   },
                new[] { S(Bon,"gold","BONUS",NO),  S(Voi,"grey","DANGER",STR) }
            }),
            new StepDevilLevelDef(38, 3, "The Flip", new[]
            {
                new[] { S(Sol,"green","SAFE",NO),  S(Voi,"red","DANGER",OK)   },
                new[] { S(Sol,"red","DANGER",NO),  S(Voi,"green","SAFE",OK)   }, // label+icon lie on both
                new[] { S(Sol,"blue","SAFE",NO),   S(Voi,"blue","DANGER",OK),  S(Voi,"gold","SAFE",OK)   },
                new[] { S(Mir,"grey","MIRROR",NO), S(Voi,"red","DANGER",REV),  S(Voi,"green","SAFE",REV) },
                new[] { S(Sol,"grey","SAFE",NO),   S(Voi,"grey","DANGER",OK),  S(Voi,"blue","SAFE",OK)   },
                new[] { S(Bon,"red","BONUS",NO),   S(Voi,"green","DANGER",STR) }
            }),
            new StepDevilLevelDef(39, 3, "All Angles", new[]
            {
                new[] { S(Sol,"red","SAFE",OK),    S(Voi,"green","DANGER",NO)  }, // colour lies
                new[] { S(Sol,"green","DANGER",OK),S(Voi,"red","SAFE",NO)      }, // label lies
                new[] { S(Sol,"green","SAFE",NO),  S(Voi,"red","DANGER",OK)    }, // icon lies
                new[] { S(Sol,"red","DANGER",NO),  S(Voi,"green","SAFE",OK),   S(Voi,"blue","SAFE",OK)   }, // col+label lie
                new[] { H(Sol),                    S(Voi,"red","DANGER",OK)    },
                new[] { S(Sol,"blue","SAFE",NO),   S(Voi,"blue","DANGER",OK)   }
            }),
            new StepDevilLevelDef(40, 3, "Three Way", new[]
            {
                new[] { S(Sol,"red","SAFE",NO),    S(Voi,"green","DANGER",OK), S(Voi,"blue","SAFE",OK)   },
                new[] { S(Sol,"green","DANGER",NO),S(Voi,"red","SAFE",OK),    S(Voi,"grey","SAFE",OK)   },
                new[] { S(Spr,"red","BOUNCE",NO),  S(Voi,"green","DANGER",OK), S(Voi,"gold","SAFE",OK)   },
                new[] { S(Sol,"blue","SAFE",NO),   S(Voi,"blue","DANGER",OK)  },
                new[] { S(Sol,"grey","DANGER",NO), S(Voi,"grey","SAFE",OK),   S(Voi,"red","SAFE",OK)    },
                new[] { S(Bon,"gold","BONUS",NO),  S(Voi,"gold","DANGER",STR),S(Voi,"blue","SAFE",STR)  }
            }),
            new StepDevilLevelDef(41, 3, "Liar's Grid", new[]
            {
                new[] { S(Sol,"red","DANGER",OK),  S(Voi,"green","SAFE",NO),   S(Voi,"blue","SAFE",NO)   },
                new[] { S(Sol,"green","SAFE",NO),  S(Voi,"green","DANGER",OK), S(Voi,"red","SAFE",OK)   },
                new[] { S(Mir,"red","SAFE",NO),    S(Voi,"green","DANGER",REV),S(Voi,"grey","SAFE",REV) },
                new[] { S(Sol,"blue","DANGER",OK), S(Voi,"blue","SAFE",NO),    S(Voi,"gold","SAFE",NO)   },
                new[] { S(Sol,"grey","SAFE",NO),   S(Voi,"grey","DANGER",OK),  S(Voi,"blue","SAFE",OK)   },
                new[] { S(Sol,"gold","DANGER",OK), S(Voi,"gold","SAFE",NO),    S(Voi,"red","SAFE",NO)    },
                new[] { S(Bon,"blue","BONUS",NO),  S(Voi,"red","DANGER",STR),  S(Voi,"green","SAFE",STR) }
            }),
            new StepDevilLevelDef(42, 3, "Boss: Icon King", new[]
            {
                new[] { S(Sol,"red","SAFE",NO),    S(Voi,"green","DANGER",OK), S(Voi,"blue","DANGER",OK) },
                new[] { S(Sol,"green","DANGER",OK),S(Voi,"red","SAFE",NO),    S(Voi,"grey","SAFE",NO)   },
                new[] { S(Sol,"blue","SAFE",NO),   S(Voi,"blue","DANGER",OK)  },
                new[] { S(Mir,"gold","MIRROR",NO), S(Voi,"red","DANGER",REV),  S(Voi,"green","SAFE",REV) },
                new[] { S(Sol,"grey","DANGER",NO), S(Voi,"grey","SAFE",OK),   S(Voi,"gold","SAFE",OK)   },
                new[] { S(Spr,"red","BOUNCE",NO),  S(Voi,"green","DANGER",OK), S(Voi,"blue","SAFE",OK)   },
                new[] { S(Sol,"gold","SAFE",NO),   S(Voi,"gold","DANGER",OK),  S(Voi,"red","SAFE",OK)   },
                new[] { S(Bon,"blue","BONUS",NO),  S(Voi,"red","DANGER",STR),  S(Voi,"grey","SAFE",STR)  }
            }),

            // ════════════════════════════════════════════════════════════
            // WORLD 5 – The Devil's Garden  (Levels 43-55)
            // TWO signals lie simultaneously. ONE is honest.
            // Patterns:
            //   col+label lie, icon honest: S(Sol,"red","DANGER",OK) / S(Voi,"green","SAFE",NO)
            //   col+icon  lie, label honest: S(Sol,"red","SAFE",NO)  / S(Voi,"green","DANGER",OK)
            //   label+icon lie, col honest:  S(Sol,"green","DANGER",NO) / S(Voi,"red","SAFE",OK)
            // ════════════════════════════════════════════════════════════

            new StepDevilLevelDef(43, 4, "Double Cross", new[]
            {
                // col+label lie → only ICON honest
                new[] { S(Sol,"red","DANGER",OK),  S(Voi,"green","SAFE",NO)  },
                new[] { S(Sol,"green","SAFE",OK),  S(Voi,"red","DANGER",NO)  },
                new[] { S(Sol,"red","DANGER",OK),  S(Voi,"green","SAFE",NO),  S(Voi,"blue","SAFE",NO)  },
                new[] { H(Sol),                    S(Voi,"green","SAFE",NO)  }
            }),
            new StepDevilLevelDef(44, 4, "Twisted Pair", new[]
            {
                // col+icon lie → only LABEL honest
                new[] { S(Sol,"red","SAFE",NO),   S(Voi,"green","DANGER",OK) },
                new[] { S(Sol,"green","SAFE",NO),  S(Voi,"red","DANGER",OK)  },
                new[] { S(Sol,"blue","SAFE",NO),   S(Voi,"blue","DANGER",OK), S(Voi,"grey","SAFE",OK)  },
                new[] { S(Bon,"red","BONUS",NO),   S(Voi,"gold","DANGER",STR)},
                new[] { S(Sol,"grey","SAFE",NO),   S(Voi,"grey","DANGER",OK) }
            }),
            new StepDevilLevelDef(45, 4, "Colour Truth", new[]
            {
                // label+icon lie → only COLOUR honest
                new[] { S(Sol,"green","DANGER",NO),S(Voi,"red","SAFE",OK)    },
                new[] { S(Sol,"green","DANGER",NO),S(Voi,"red","SAFE",OK)    },
                new[] { S(Sol,"blue","DANGER",NO), S(Voi,"red","SAFE",OK),   S(Voi,"blue","SAFE",OK)  },
                new[] { S(Spr,"yellow","DANGER",OK),S(Voi,"red","SAFE",NO)  },  // Spr: col honest, label+icon lie
                new[] { S(Sol,"green","DANGER",NO),S(Voi,"red","SAFE",OK)    }
            }),
            new StepDevilLevelDef(46, 4, "One Truth", new[]
            {
                new[] { S(Sol,"red","DANGER",OK),  S(Voi,"green","SAFE",NO)  },  // icon honest
                new[] { S(Sol,"red","SAFE",NO),    S(Voi,"green","DANGER",OK) },  // label honest
                new[] { S(Sol,"green","DANGER",NO),S(Voi,"red","SAFE",OK)    },  // colour honest
                new[] { S(Sol,"red","DANGER",OK),  S(Voi,"green","SAFE",NO),  S(Voi,"blue","SAFE",NO)  },
                new[] { S(Sol,"red","SAFE",NO),    S(Voi,"green","DANGER",OK) }
            }),
            new StepDevilLevelDef(47, 4, "Garden Path", new[]
            {
                new[] { S(Sol,"red","DANGER",OK),  S(Voi,"green","SAFE",NO),  S(Voi,"blue","SAFE",NO)  },
                new[] { S(Sol,"blue","SAFE",NO),   S(Voi,"blue","DANGER",OK) },
                new[] { S(Sol,"red","DANGER",OK),  S(Voi,"green","SAFE",NO)  },
                new[] { S(Mir,"red","SAFE",NO),    S(Voi,"green","DANGER",REV),S(Voi,"grey","SAFE",REV) },
                new[] { S(Sol,"green","DANGER",NO),S(Voi,"red","SAFE",OK)    },
                new[] { S(Bon,"red","DANGER",STR), S(Voi,"green","SAFE",NO)  }   // Bon col+label lie, STR honest
            }),
            new StepDevilLevelDef(48, 4, "Devil Mix", new[]
            {
                new[] { S(Sol,"red","DANGER",OK),  S(Voi,"green","SAFE",NO),  S(Voi,"red","DANGER",NO) },
                new[] { S(Sol,"green","DANGER",NO),S(Voi,"red","SAFE",OK)    },
                new[] { S(Sol,"blue","SAFE",NO),   S(Voi,"blue","DANGER",OK),  S(Voi,"grey","SAFE",OK)  },
                new[] { S(Spr,"red","DANGER",OK),  S(Voi,"green","SAFE",NO),  S(Voi,"gold","SAFE",NO)  },
                new[] { S(Sol,"grey","SAFE",NO),   S(Voi,"grey","DANGER",OK) },
                new[] { S(Bon,"gold","DANGER",STR),S(Voi,"red","SAFE",NO)    },
                new[] { S(Sol,"red","DANGER",OK),  S(Voi,"blue","SAFE",NO),   S(Voi,"green","SAFE",NO) }
            }),
            new StepDevilLevelDef(49, 4, "Double Agents", new[]
            {
                new[] { S(Sol,"red","DANGER",OK),  S(Voi,"green","SAFE",NO)  },
                new[] { S(Sol,"red","SAFE",NO),    S(Voi,"green","DANGER",OK) },
                new[] { S(Sol,"green","DANGER",NO),S(Voi,"red","SAFE",OK)    },
                new[] { S(Sol,"red","DANGER",OK),  S(Voi,"green","SAFE",NO),  S(Voi,"blue","SAFE",NO)  },
                new[] { S(Sol,"blue","SAFE",NO),   S(Voi,"blue","DANGER",OK) },
                new[] { S(Sol,"grey","DANGER",OK), S(Voi,"grey","SAFE",NO)   }
            }),
            new StepDevilLevelDef(50, 4, "Chaos Theory", new[]
            {
                new[] { S(Sol,"red","DANGER",OK),  S(Voi,"green","SAFE",NO),  S(Voi,"red","DANGER",NO) },
                new[] { S(Sol,"green","DANGER",NO),S(Voi,"red","SAFE",OK)    },
                new[] { S(Sol,"red","SAFE",NO),    S(Voi,"green","DANGER",OK) },
                new[] { S(Mir,"red","SAFE",NO),    S(Voi,"green","DANGER",REV),S(Voi,"blue","SAFE",REV) },
                new[] { S(Sol,"blue","DANGER",OK), S(Voi,"blue","SAFE",NO),   S(Voi,"grey","SAFE",NO)  },
                new[] { S(Bon,"red","DANGER",STR), S(Voi,"green","SAFE",NO)  }
            }),
            new StepDevilLevelDef(51, 4, "Noise Floor", new[]
            {
                new[] { S(Sol,"red","DANGER",OK),  S(Voi,"green","SAFE",NO),  S(Voi,"blue","SAFE",NO)  },
                new[] { S(Sol,"blue","SAFE",NO),   S(Voi,"blue","DANGER",OK) },
                new[] { S(Sol,"grey","DANGER",OK), S(Voi,"red","SAFE",NO),   S(Voi,"green","SAFE",NO) },
                new[] { S(Spr,"red","DANGER",OK),  S(Voi,"green","SAFE",NO)  },
                new[] { S(Sol,"red","SAFE",NO),    S(Voi,"green","DANGER",OK) },
                new[] { S(Sol,"gold","DANGER",OK), S(Voi,"gold","SAFE",NO),   S(Voi,"red","SAFE",NO)   },
                new[] { S(Bon,"blue","DANGER",STR),S(Voi,"blue","SAFE",NO)   }
            }),
            new StepDevilLevelDef(52, 4, "Hell Garden", new[]
            {
                new[] { S(Sol,"red","DANGER",OK),  S(Voi,"green","SAFE",NO)  },
                new[] { S(Sol,"green","SAFE",NO),  S(Voi,"red","DANGER",OK),  S(Voi,"blue","SAFE",OK)  },
                new[] { S(Sol,"red","SAFE",NO),    S(Voi,"green","DANGER",OK) },
                new[] { S(Sol,"blue","DANGER",OK), S(Voi,"blue","SAFE",NO),   S(Voi,"grey","SAFE",NO)  },
                new[] { S(Sol,"grey","SAFE",NO),   S(Voi,"grey","DANGER",OK) },
                new[] { S(Bon,"red","DANGER",STR), S(Voi,"green","SAFE",NO)  },
                new[] { S(Sol,"gold","DANGER",OK), S(Voi,"red","SAFE",NO),   S(Voi,"blue","SAFE",NO)  }
            }),
            new StepDevilLevelDef(53, 4, "Garden of Lies", new[]
            {
                new[] { S(Sol,"red","DANGER",OK),  S(Voi,"green","SAFE",NO),  S(Voi,"red","DANGER",NO) },
                new[] { S(Sol,"blue","SAFE",NO),   S(Voi,"blue","DANGER",OK) },
                new[] { S(Mir,"red","SAFE",NO),    S(Voi,"green","DANGER",REV),S(Voi,"grey","SAFE",REV) },
                new[] { S(Sol,"grey","DANGER",OK), S(Voi,"grey","SAFE",NO),   S(Voi,"gold","SAFE",NO)  },
                new[] { S(Sol,"red","DANGER",OK),  S(Voi,"green","SAFE",NO)  },
                new[] { S(Spr,"red","DANGER",OK),  S(Voi,"green","SAFE",NO),  S(Voi,"blue","SAFE",NO)  },
                new[] { S(Sol,"gold","SAFE",NO),   S(Voi,"gold","DANGER",OK) },
                new[] { S(Bon,"blue","DANGER",STR),S(Voi,"red","SAFE",NO)    }
            }),
            new StepDevilLevelDef(54, 4, "The Devil Blooms", new[]
            {
                new[] { S(Sol,"red","DANGER",OK),  S(Voi,"green","SAFE",NO)  },
                new[] { S(Sol,"green","SAFE",NO),  S(Voi,"red","DANGER",OK)  },
                new[] { S(Sol,"red","SAFE",NO),    S(Voi,"green","DANGER",OK) },
                new[] { S(Sol,"blue","DANGER",OK), S(Voi,"blue","SAFE",NO),   S(Voi,"grey","SAFE",NO)  },
                new[] { S(Sol,"grey","SAFE",NO),   S(Voi,"grey","DANGER",OK) },
                new[] { S(Sol,"red","DANGER",OK),  S(Voi,"green","SAFE",NO),  S(Voi,"blue","SAFE",NO)  },
                new[] { S(Sol,"gold","SAFE",NO),   S(Voi,"gold","DANGER",OK) },
                new[] { S(Bon,"red","DANGER",STR), S(Voi,"green","SAFE",NO)  }
            }),
            new StepDevilLevelDef(55, 4, "Boss: Garden Lord", new[]
            {
                new[] { S(Sol,"red","DANGER",OK),  S(Voi,"green","SAFE",NO),  S(Voi,"blue","SAFE",NO)  },
                new[] { S(Sol,"green","SAFE",NO),  S(Voi,"red","DANGER",OK)  },
                new[] { S(Sol,"red","SAFE",NO),    S(Voi,"green","DANGER",OK), S(Voi,"grey","SAFE",OK)  },
                new[] { S(Mir,"red","DANGER",NO),  S(Voi,"green","SAFE",REV),  S(Voi,"blue","SAFE",REV) },
                new[] { S(Sol,"blue","DANGER",OK), S(Voi,"blue","SAFE",NO)   },
                new[] { S(Sol,"grey","SAFE",NO),   S(Voi,"grey","DANGER",OK), S(Voi,"gold","SAFE",OK)  },
                new[] { S(Spr,"red","DANGER",OK),  S(Voi,"green","SAFE",NO),  S(Voi,"red","DANGER",NO) },
                new[] { S(Bon,"gold","DANGER",STR),S(Voi,"red","SAFE",NO)    },
                new[] { S(Sol,"gold","SAFE",NO),   S(Voi,"gold","DANGER",OK), S(Voi,"blue","SAFE",OK)  }
            }),

            // ════════════════════════════════════════════════════════════
            // WORLD 6 – Abyss  (Levels 56-65)
            // ALL THREE signals may lie simultaneously.
            // S(Sol,"red","DANGER",NO) = Solid with ALL THREE lies.
            // S(Voi,"green","SAFE",OK) = Void  with ALL THREE lies.
            // Player must rely on cross-fork pattern memory.
            // ════════════════════════════════════════════════════════════

            new StepDevilLevelDef(56, 5, "Into the Dark", new[]
            {
                // Intro: mostly two-lie combos, one all-three
                new[] { S(Sol,"red","DANGER",NO),  S(Voi,"green","SAFE",OK)   }, // ALL THREE lie
                new[] { S(Sol,"red","DANGER",OK),  S(Voi,"green","SAFE",NO)   }, // icon honest
                new[] { S(Sol,"red","DANGER",NO),  S(Voi,"green","SAFE",OK)   },
                new[] { H(Sol),                    S(Voi,"green","SAFE",OK)   }
            }),
            new StepDevilLevelDef(57, 5, "Full Darkness", new[]
            {
                new[] { S(Sol,"red","DANGER",NO),  S(Voi,"green","SAFE",OK),   S(Voi,"blue","SAFE",OK)  },
                new[] { S(Sol,"red","DANGER",NO),  S(Voi,"green","SAFE",OK)   },
                new[] { S(Sol,"blue","DANGER",NO), S(Voi,"blue","SAFE",OK)    },
                new[] { S(Sol,"red","DANGER",OK),  S(Voi,"green","SAFE",NO)   }, // icon honest
                new[] { S(Sol,"red","DANGER",NO),  S(Voi,"green","SAFE",OK),   S(Voi,"grey","SAFE",OK)  }
            }),
            new StepDevilLevelDef(58, 5, "Blind Walk", new[]
            {
                new[] { S(Sol,"red","DANGER",NO),  S(Voi,"green","SAFE",OK)   },
                new[] { S(Sol,"green","SAFE",OK),  S(Voi,"red","DANGER",NO)   }, // all three lie on void
                new[] { S(Sol,"red","DANGER",NO),  S(Voi,"green","SAFE",OK),   S(Voi,"blue","SAFE",OK)  },
                new[] { S(Bon,"red","DANGER",NO),  S(Voi,"gold","SAFE",STR)   }, // Bonus all-lie; Void icon lie
                new[] { S(Sol,"blue","DANGER",NO), S(Voi,"blue","SAFE",OK)    },
                new[] { S(Sol,"red","DANGER",NO),  S(Voi,"green","SAFE",OK)   }
            }),
            new StepDevilLevelDef(59, 5, "Abyss Gazer", new[]
            {
                new[] { S(Sol,"red","DANGER",NO),  S(Voi,"green","SAFE",OK),   S(Voi,"blue","SAFE",OK)  },
                new[] { S(Sol,"green","SAFE",OK),  S(Voi,"red","DANGER",NO)   },
                new[] { S(Sol,"blue","DANGER",NO), S(Voi,"blue","SAFE",OK)    },
                new[] { S(Spr,"red","DANGER",NO),  S(Voi,"green","SAFE",OK),   S(Voi,"grey","SAFE",OK)  },
                new[] { S(Sol,"red","DANGER",NO),  S(Voi,"green","SAFE",OK)   },
                new[] { S(Sol,"grey","DANGER",NO), S(Voi,"grey","SAFE",OK),    S(Voi,"gold","SAFE",OK)  },
                new[] { S(Bon,"red","DANGER",NO),  S(Voi,"gold","SAFE",STR)   }
            }),
            new StepDevilLevelDef(60, 5, "Nothing Is True", new[]
            {
                new[] { S(Sol,"red","DANGER",NO),  S(Voi,"green","SAFE",OK)   },
                new[] { S(Sol,"green","SAFE",OK),  S(Voi,"red","DANGER",NO),   S(Voi,"blue","DANGER",NO)},
                new[] { S(Sol,"red","DANGER",NO),  S(Voi,"blue","SAFE",OK)    },
                new[] { S(Mir,"red","DANGER",NO),  S(Voi,"green","SAFE",REV),  S(Voi,"grey","SAFE",REV) },
                new[] { S(Sol,"grey","DANGER",NO), S(Voi,"grey","SAFE",OK)    },
                new[] { S(Sol,"red","DANGER",NO),  S(Voi,"green","SAFE",OK),   S(Voi,"blue","SAFE",OK)  },
                new[] { S(Sol,"gold","DANGER",NO), S(Voi,"gold","SAFE",OK)    }
            }),
            new StepDevilLevelDef(61, 5, "The Void Stares", new[]
            {
                new[] { S(Sol,"red","DANGER",NO),  S(Voi,"green","SAFE",OK),   S(Voi,"red","DANGER",OK) },
                new[] { S(Sol,"green","SAFE",OK),  S(Voi,"red","DANGER",NO)   },
                new[] { S(Sol,"blue","DANGER",NO), S(Voi,"blue","SAFE",OK),    S(Voi,"grey","SAFE",OK)  },
                new[] { S(Sol,"red","DANGER",NO),  S(Voi,"green","SAFE",OK)   },
                new[] { S(Spr,"red","DANGER",NO),  S(Voi,"green","SAFE",OK),   S(Voi,"blue","SAFE",OK)  },
                new[] { S(Sol,"grey","DANGER",NO), S(Voi,"grey","SAFE",OK)    },
                new[] { S(Sol,"red","DANGER",NO),  S(Voi,"green","SAFE",OK),   S(Voi,"gold","SAFE",OK)  },
                new[] { S(Bon,"red","DANGER",NO),  S(Voi,"green","SAFE",STR)  }
            }),
            new StepDevilLevelDef(62, 5, "Devil's Core", new[]
            {
                new[] { S(Sol,"red","DANGER",NO),  S(Voi,"green","SAFE",OK)   },
                new[] { S(Sol,"green","SAFE",OK),  S(Voi,"red","DANGER",NO),   S(Voi,"blue","DANGER",NO)},
                new[] { S(Sol,"blue","DANGER",NO), S(Voi,"blue","SAFE",OK)    },
                new[] { S(Sol,"red","DANGER",NO),  S(Voi,"green","SAFE",OK),   S(Voi,"grey","SAFE",OK)  },
                new[] { S(Mir,"red","DANGER",NO),  S(Voi,"green","SAFE",REV),  S(Voi,"blue","SAFE",REV) },
                new[] { S(Sol,"grey","DANGER",NO), S(Voi,"grey","SAFE",OK)    },
                new[] { S(Sol,"gold","DANGER",NO), S(Voi,"gold","SAFE",OK),    S(Voi,"red","SAFE",OK)   },
                new[] { S(Sol,"red","DANGER",NO),  S(Voi,"green","SAFE",OK)   },
                new[] { S(Bon,"red","DANGER",NO),  S(Voi,"gold","SAFE",STR)   }
            }),
            new StepDevilLevelDef(63, 5, "Absolute Chaos", new[]
            {
                new[] { S(Sol,"red","DANGER",NO),  S(Voi,"green","SAFE",OK),   S(Voi,"blue","SAFE",OK)  },
                new[] { S(Sol,"green","SAFE",OK),  S(Voi,"red","DANGER",NO)   },
                new[] { S(Sol,"blue","DANGER",NO), S(Voi,"grey","SAFE",OK),    S(Voi,"gold","SAFE",OK)  },
                new[] { S(Spr,"red","DANGER",NO),  S(Voi,"green","SAFE",OK)   },
                new[] { S(Sol,"grey","DANGER",NO), S(Voi,"grey","SAFE",OK),    S(Voi,"blue","SAFE",OK)  },
                new[] { S(Sol,"red","DANGER",NO),  S(Voi,"green","SAFE",OK)   },
                new[] { S(Sol,"gold","DANGER",NO), S(Voi,"gold","SAFE",OK),    S(Voi,"red","SAFE",OK)   },
                new[] { S(Bon,"red","DANGER",NO),  S(Voi,"blue","SAFE",STR),   S(Voi,"green","SAFE",STR)},
                new[] { S(Sol,"blue","DANGER",NO), S(Voi,"blue","SAFE",OK)    }
            }),
            new StepDevilLevelDef(64, 5, "The Final Lie", new[]
            {
                new[] { S(Sol,"red","DANGER",NO),  S(Voi,"green","SAFE",OK)   },
                new[] { S(Sol,"green","SAFE",OK),  S(Voi,"red","DANGER",NO),   S(Voi,"blue","DANGER",NO)},
                new[] { S(Sol,"blue","DANGER",NO), S(Voi,"blue","SAFE",OK),    S(Voi,"grey","SAFE",OK)  },
                new[] { S(Sol,"red","DANGER",NO),  S(Voi,"green","SAFE",OK)   },
                new[] { S(Mir,"red","DANGER",NO),  S(Voi,"green","SAFE",REV),  S(Voi,"gold","SAFE",REV) },
                new[] { S(Sol,"grey","DANGER",NO), S(Voi,"grey","SAFE",OK)    },
                new[] { S(Sol,"red","DANGER",NO),  S(Voi,"blue","SAFE",OK),    S(Voi,"green","SAFE",OK) },
                new[] { S(Spr,"red","DANGER",NO),  S(Voi,"green","SAFE",OK)   },
                new[] { S(Sol,"gold","DANGER",NO), S(Voi,"gold","SAFE",OK),    S(Voi,"red","SAFE",OK)   }
            }),
            new StepDevilLevelDef(65, 5, "Boss: The Devil", new[]
            {
                new[] { S(Sol,"red","DANGER",NO),  S(Voi,"green","SAFE",OK),   S(Voi,"blue","SAFE",OK)  },
                new[] { S(Sol,"green","SAFE",OK),  S(Voi,"red","DANGER",NO)   },
                new[] { S(Sol,"blue","DANGER",NO), S(Voi,"blue","SAFE",OK)    },
                new[] { S(Sol,"red","DANGER",NO),  S(Voi,"green","SAFE",OK),   S(Voi,"grey","SAFE",OK)  },
                new[] { S(Mir,"red","DANGER",NO),  S(Voi,"green","SAFE",REV),  S(Voi,"blue","SAFE",REV) },
                new[] { S(Sol,"grey","DANGER",NO), S(Voi,"grey","SAFE",OK)    },
                new[] { S(Spr,"red","DANGER",NO),  S(Voi,"green","SAFE",OK),   S(Voi,"gold","SAFE",OK)  },
                new[] { S(Sol,"gold","DANGER",NO), S(Voi,"red","SAFE",OK)     },
                new[] { S(Bon,"red","DANGER",NO),  S(Voi,"green","SAFE",STR),  S(Voi,"blue","SAFE",STR) },
                new[] { S(Sol,"red","DANGER",NO),  S(Voi,"green","SAFE",OK),   S(Voi,"red","DANGER",OK) }
            })
        };

        public static StepDevilLevelDef GetLevel(int zeroBasedIndex) => Levels[zeroBasedIndex];
    }
}
