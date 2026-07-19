# Gameplay

Long-range, methodical, ~Splispace. Might include options for faster-paced gameplay.

## Defense

Relies on evasion, point-defense, and managing your signature. 

Each ship has a scan range and a signature. Signature increases when firing certain weapons, and decreases each tick, down to a set minimum. Ship A can 
accurately target ship B with turrets and fire missiles at it if dist(A,B) <= scan(A) + signature(B). The idea is that holding fire is roughly analoguous to taking cover.

Point defense can shoot down missiles (maybe all projectiles?), and can engage ships as well. PD turrets will prioritize targets as follows:
* missiles targeting the ship based on angle and distance
* other ships/missiles based on angle and distance

Turrets have a limited rotation speed, and will attempt to predict the target's position. As of yet, do not account for angular velocity. All projectiles have travel time, allowing the target to dodge them.

## Offense

Ramming is a no-no.

Turrets increase their rate of fire the longer they are shooting, and rapidly decrease it when not. Turrets gain increased accuracy when the owner is within optimal engagement range to target.

Missiles gain increased speed and tracking when the shooter is facing the target (use dot-product to calculate the strength of this bonus).

## Morale

Ships suffer morale damage when friendlies are destroyed. When a ship's morale reaches 0, it will flee from its current target. Morale recovers over time, but it cannot recover for a certain period of time after taking morale damage.

## Player awareness

Enemies are colored in red, friendlies in blue, neutrals(assuming they exist) in white. Enemies beyond optimal engagement range have transluscent bounding boxes instead of normal ones. An enemy's target is displayed upon selecting them. Enemies within optimal engagement range and targeting the player will have a thin red line between the two. Fleeing ships have flickering bounding boxes.

# Campaign

Hand-made, maybe with some randomness elements. The final battle being a massive fleet battle with randomly generated fleets might work. Use a similar menu to Halo MCC.
