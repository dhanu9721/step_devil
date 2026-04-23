using UnityEngine;

namespace StepDevil
{
    /// <summary>
    /// Pool of misleading psychological hints shown to the player during forks.
    /// Grouped by manipulation category so the game can mix them realistically.
    /// </summary>
    public static class SDFakeHintBank
    {
        // ── Social Proof ──────────────────────────────────────────────────────
        static readonly string[] SocialProof =
        {
            "Most players pick LEFT here.",
            "Most players pick RIGHT here.",
            "9 out of 10 players get this one right.",
            "Everyone picks the brighter one.",
            "The crowd always knows which is safe.",
            "Beginners always pick the wrong side.",
            "Smart players never hesitate here.",
            "The top players always go left first.",
        };

        // ── False Patterns ────────────────────────────────────────────────────
        static readonly string[] FalsePatterns =
        {
            "Safe stones always alternate sides.",
            "The safe stone is never on the same side twice.",
            "Patterns repeat every 3 steps — you're on step 3.",
            "After a VOID, the next one is always safe.",
            "The first stone in each fork is always honest.",
            "Two lies in a row? The next stone tells the truth.",
            "Safe stones cluster on the LEFT in odd-numbered forks.",
            "The devil never places danger on the right twice.",
        };

        // ── False Authority ───────────────────────────────────────────────────
        static readonly string[] FalseAuthority =
        {
            "PRO TIP: The label is always honest here.",
            "PRO TIP: Colors never lie in this world.",
            "PRO TIP: The icon is the only thing you can trust.",
            "HINT: The brighter the stone, the safer it is.",
            "HINT: Gold outline means guaranteed safe.",
            "HINT: The devil only lies about labels, not colors.",
            "HINT: Icons are always truthful — read them carefully.",
            "REMINDER: The devil skips lying on bonus stones.",
        };

        // ── Overconfidence ────────────────────────────────────────────────────
        static readonly string[] Overconfidence =
        {
            "This one is obvious — don't overthink it.",
            "Trust your eyes. What you see is what you get.",
            "You already know which one is right.",
            "The safe path is clear if you just look.",
            "No tricks here. Pick what feels natural.",
            "Simple fork. First instinct is always correct.",
            "The answer is staring right at you.",
            "Some forks are just easy — this is one of them.",
        };

        // ── Reverse Psychology ────────────────────────────────────────────────
        static readonly string[] ReversePsychology =
        {
            "Don't pick the one that looks safe.",
            "The obvious choice is almost always wrong.",
            "Avoid anything that looks too inviting.",
            "The safer it looks, the more suspicious you should be.",
            "If it feels right, it probably isn't.",
            "The devil wants you to trust the pretty one.",
            "The loud stone is hiding something.",
            "Never trust what stands out.",
        };

        // ── Gambler's Fallacy ─────────────────────────────────────────────────
        static readonly string[] GamblersFallacy =
        {
            "You're on a streak — luck is on your side.",
            "After so many correct steps, you've earned a safe one.",
            "The devil can't fool you forever.",
            "Statistically, you're due for an easy one.",
            "You haven't fallen in a while — keep the streak going.",
            "Momentum is real. Trust the flow.",
            "You've been so careful — relax a little.",
            "After a fall, the game gets easier. Fact.",
        };

        // ── Urgency / Pressure ────────────────────────────────────────────────
        static readonly string[] Urgency =
        {
            "Time is running out — just pick one!",
            "Hesitation is the real trap here.",
            "The longer you wait, the less time to think.",
            "Quick decisions are usually the right ones.",
            "Don't let the timer make you panic — or do.",
            "Fast players never second-guess themselves.",
            "The clock doesn't care which you pick.",
            "Overthinking costs you everything.",
        };

        static readonly string[][] AllCategories =
        {
            SocialProof,
            FalsePatterns,
            FalseAuthority,
            Overconfidence,
            ReversePsychology,
            GamblersFallacy,
            Urgency,
        };

        static readonly string[] CategoryPrefixes =
        {
            "\U0001F465",  // 👥 social
            "\U0001F4CA",  // 📊 pattern
            "\U0001F4A1",  // 💡 authority
            "\U0001F60F",  // 😏 confident
            "\U0001F914",  // 🤔 reverse
            "\U0001F3B2",  // 🎲 gambler
            "\u23F1\uFE0F", // ⏱ urgency
        };

        /// <summary>
        /// Returns a random fake hint string, or null if the hint bank decides
        /// to stay silent (roughly 35% of the time).
        /// </summary>
        public static string Pick(int worldIndex, int forkIndex)
        {
            // World 1 (Training Ground) — no fake hints, it's the tutorial
            if (worldIndex == 0) return null;

            // First fork of any level — lighter chance (player is orienting)
            var roll = Random.value;
            var threshold = forkIndex == 0 ? 0.35f : 0.65f;
            if (roll > threshold) return null;

            var catIdx  = Random.Range(0, AllCategories.Length);
            var pool    = AllCategories[catIdx];
            var hint    = pool[Random.Range(0, pool.Length)];
            var prefix  = CategoryPrefixes[catIdx];

            return $"{prefix} {hint}";
        }
    }
}
