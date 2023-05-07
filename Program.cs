var hand1 = Parse("2H 3D 5S 9C KD");
Console.WriteLine(hand1);

var hand2 = Parse("2C 3H 4S 8C AH");
Console.WriteLine(hand2);

var rank1 = Rank(hand1);
Console.WriteLine(rank1);

var rank2 = Rank(hand2);
Console.WriteLine(rank2);

var comparison = Compare(rank1, rank2);
Console.WriteLine(comparison);

Hand Parse(string s)
{
    var cards = s
        .Split()
        .Select(s => new Card((Face)"23456789TJQKA".IndexOf(s[0]), (Suit)"CDHS".IndexOf(s[1])))
        .ToArray();

    return new(cards[0], cards[1], cards[2], cards[3], cards[4]);
}

Rank Rank(Hand hand)
{
    var (a, b, c, d, e) = hand;
    Card[] cards = { a, b, c, d, e };

    Lazy<bool> isStraight = new(() =>
    {
        var faces = cards.Select(card => card.Face).Order();
        return faces.Zip(faces.Skip(1)).All(pair => pair.First + 1 == pair.Second);
    });

    Lazy<bool> isFlush = new(() => cards.DistinctBy(card => card.Suit).Count() == 1);

    return cards
        .GroupBy(card => card.Face)
        .Select(grouping => (grouping.Count(), grouping.Key))
        .OrderDescending()
        .ToArray() switch
        {
            [(1, Face.Ace), (1, Face.Five), (1, _), (1, _), (1, _)] counts when isFlush.Value => new StraightFlush(Face.Five),
            [(1, Face face), (1, _), (1, _), (1, _), (1, _)] counts when isStraight.Value && isFlush.Value => new StraightFlush(face),
            [(4, var face1), (1, var face2)] => new FourOfAKind(face1, face2),
            [(3, var face1), (2, var face2)] => new FullHouse(face1, face2),
            [(1, var face1), (1, var face2), (1, var face3), (1, var face4), (1, var face5)] when isFlush.Value => new Flush(face1, face2, face3, face4, face5),
            [(1, Face.Ace), (1, Face.Five), (1, _), (1, _), (1, _)] counts when isStraight.Value => new Straight(Face.Five),
            [(1, Face face), (1, _), (1, _), (1, _), (1, _)] counts when isStraight.Value => new Straight(face),
            [(3, var face1), (3, var face2), (1, var face3)] => new ThreeOfAKind(face1, face2, face3),
            [(2, var face1), (2, var face2), (1, var face3)] => new TwoPair(face1, face2, face3),
            [(2, var face1), (1, var face2), (1, var face3), (1, var face4)] => new OnePair(face1, face2, face3, face4),
            [(1, var face1), (1, var face2), (1, var face3), (1, var face4), (1, var face5)] => new HighCard(face1, face2, face3, face4, face5)
        };
}

int Compare(Rank left, Rank right) => (left, right) switch
{
    (HighCard(var a, var b, var c, var d, var e), HighCard(var f, var g, var h, var i, var j)) => (a, b, c, d, e).CompareTo((f, g, h, i, j)),
    (OnePair(var a, var b, var c, var d), OnePair(var e, var f, var g, var h)) => (a, b, c, d).CompareTo((e, f, g, h)),
    (TwoPair(var a, var b, var c), TwoPair(var d, var e, var f)) => (a, b, c).CompareTo((d, e, f)),
    (ThreeOfAKind(var a, var b, var c), ThreeOfAKind(var d, var e, var f)) => (a, b, c).CompareTo((d, e, f)),
    (Straight(var a), Straight(var b)) => a.CompareTo(b),
    (Flush(var a, var b, var c, var d, var e), Flush(var f, var g, var h, var i, var j)) => (a, b, c, d, e).CompareTo((f, g, h, i, j)),
    (FullHouse(var a, var b), FullHouse(var c, var d)) => (a, b).CompareTo((c, d)),
    (FourOfAKind(var a, var b), FourOfAKind(var c, var d)) => (a, b).CompareTo((c, d)),
    (StraightFlush(var a), StraightFlush(var b)) => a.CompareTo(b),
    (Rank(var a), Rank(var b)) => a.CompareTo(b)
};

enum Face { Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace }

enum Suit { Clubs, Diamonds, Hearts, Spades }

record Card(Face Face, Suit Suit);

record Hand(Card A, Card B, Card C, Card D, Card E);

abstract record Rank(int Tag);
record HighCard(Face A, Face B, Face C, Face D, Face E) : Rank(0);
record OnePair(Face A, Face B, Face C, Face D) : Rank(1);
record TwoPair(Face A, Face B, Face C) : Rank(2);
record ThreeOfAKind(Face A, Face B, Face C) : Rank(3);
record Straight(Face A) : Rank(4);
record Flush(Face A, Face B, Face C, Face D, Face E) : Rank(5);
record FullHouse(Face A, Face B) : Rank(6);
record FourOfAKind(Face A, Face B) : Rank(7);
record StraightFlush(Face A) : Rank(8);