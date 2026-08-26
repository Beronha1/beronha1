es-objective-summary-fmt = {$name}: {$success ->
    [true] [color=limegreen]Success[/color]
    *[false] [color=red]Failed[/color]
} {$percent ->
    [0] {""}
    [100] {""}
    *[other] ([color=gray]{$percent}%[/color])
}

es-objective-text-organization = Team
es-objective-tooltip-organization = This is a [bold]shared organization objective[/bold].

    All members of your organization share this objective and must work together. Objective completion is shared between everyone who has it assigned.

es-objective-text-secret-identity = Solo
es-objective-tooltip-secret-identity = This is a [bold]personal secret identity objective[/bold].

    This is a unique objective based on the secret identity that you are assigned. Only you can view this objective. Other members of your organization may have different personal objectives.

es-fruit-vendor-objective-title = Feed summoned fruit to {$count} people
es-arms-dealer-objective-title = Have {$count} summoned firearms held by crew at the end of the shift
es-guzzle-objective-title = Drink {$count} units of liquid
es-imbibe-reagent-guzzler-desc = Drink as much {$reagent} as you can.
es-guzzle-specific-objective-title = Drink {$count} units of {$reagent}
es-guzzle-unique-reagents-objective-title = Drink {$count} unique reagents
es-daredevil-objective-title = Take {$count} {$damagetype} damage
es-daredevil-desc = Take as much {$damagetype} damage as you can.
es-daredevil-total-objective-title = Take {$count} total damage
es-daredevil-total-objective-desc = Put yourself in danger and accumulate damage from any source.
es-eat-unique-foods-objective-title = Eat {$count} unique foods
es-eat-food-objective-title = Eat {$name}
es-sacrifice-objective-title = Heal {$count} people by sacrificing yourself
es-sacrifice-popup-heroic-sacrifice = {$name} made a heroic sacrifice!

es-daredevil-source-objective-title-ESGrille = Take {$count} damage from a grille
es-daredevil-source-objective-desc-ESGrille = Let an electrified grille hurt you.
es-daredevil-source-objective-title-ESEmitter = Take {$count} damage from an emitter
es-daredevil-source-objective-desc-ESEmitter = Put yourself in the path of an emitter beam.
es-daredevil-source-objective-title-ESAncestor = Take {$count} damage from an ancestor
es-daredevil-source-objective-desc-ESAncestor = Provoke an ancestor and endure its attacks.

ent-ESObjectiveResearchTelescience = Research telescience
    .desc = Work with the science team to teleport the station out of the path of the coming radiation storm.
ent-ESObjectiveEatSummonedFruit = Feed summoned fruit
    .desc = Use your ability to manifest delicious fruits and feed them to people on the station.
ent-ESObjectiveHoldSummonedGuns = Arm the crew
    .desc = Summon firearms and make sure crew members hold onto them until the end of the shift.
ent-ESObjectiveKillNonCrew = Kill a hostile outsider
    .desc = Use your trusty weapon to kill someone aboard the station who is not a crew member.
ent-ESObjectiveGuzzle = Guzzle
    .desc = Consume as much liquid as you can, from anywhere you can get it!
ent-ESObjectiveGuzzleSpecialInterest = Guzzle a special interest
    .desc = Consume as much of your special-interest reagent as you can.
ent-ESObjectiveGuzzleUniqueReagents = Sample unique liquids
    .desc = Consume as many different liquids as you can.
ent-ESObjectiveTakeDamage = Take damage
    .desc = Take as much of a certain damage type as you can.
ent-ESObjectiveTakeDamageFromSource = Take damage from a source
    .desc = Take as much damage from a specified source as you can.
ent-ESObjectiveTakeTotalDamage = Take total damage
    .desc = Accumulate damage from any source.
ent-ESObjectiveEatUniqueFoods = Eat unique foods
    .desc = Your stomach yearns for the unknown. Seek out all the different tastes of the world!
ent-ESObjectiveEatFood = Satisfy an exotic craving
    .desc = Find and eat the particular food you crave.
ent-ESObjectiveSurvive = Survive
    .desc = Survive until the end of the shift at all costs.
ent-ESObjectiveSacrificeHeal = Sacrifice yourself
    .desc = Heal others by dying near them.
