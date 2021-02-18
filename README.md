# Solving Sudoko as a Constraint Satisfaction Problem (CSP) with Forward Propagation

This project is a constraint satisfaction problem (CSP) solver which is adjusted to solve Sudoku puzzles. As a result, this solver is optimized to solve `AllDiff` constraints, and is also suitable for the map coloring problem, and the N-queens problem.

![Example](img1.png)

The Sudoku puzzle is provided as a single string consisting of 81 characters, e.g.:

`.8.4..6............4.6...1.6.35...41......7....8....35.6..8..7....3.54.6.2..1..8.`

It supports either `0` or `.`, or actually any other character, as the specifier for an empty field.

This website can be used (https://qqwing.com/generate.html) to generate more Sudoku strings, just set the output to single line.

## CSP

A constraint satisfaction problem (CSP) is described in the following way:

 * Domains `D`, which are the values that can be assigned to the variable. In the case of Sudoku, the domain is `{1, 2, 3, 4, 5, 6, 7, 8, 9}`.
 * Variables `V` holds a value and a domain. In the case of Sudoku, there are `81` variables for each cell.
 * Contraint `C` is a list of constraints that must be satisfied. In the case of Sudoku, each row/column/square is constrained in a `AllDiff(...)` constraint. This gives a total of `27` constraints.

## Algorithm

It uses a recursive backtracking algorithm with forward propagation to solve the CSP problem. The backtracking procedure works like this:

```
function Search
    if all variables are set, check if the solution is consistent
        if the solution is consistent, then we have found a solution
    
    get the next variable V
    for each value in the domain of V
        assign the value to V
        recursively call search to continue to the next variable
        unassign the value from V (backtracking step)
```

This is the gist of the algorithm. The next sections will explain a few adjustments that have been made to this to increase the speed of the solver.

### Forward propagation

If we assign a value to a variable, we know that any cell in that row/column/square can't use this value anymore. Using this, we can remove this value from all the other variable domains that share the same constraint. If it happens that a domain becomes empty, we know that we have an infeasible solution, and we can backtrack early. This entire proces is known as forward propagation.

The algorithm uses a `ConstraintPropagator` which will propagate the value after a variable is set. The propagator will return a `Propagation` object with information of which values have been removed (`Reduction`) from the domains of other variables. This is required, because if we unset the value of the variable, we also need to restore the domains which we have reduced.

Another thing that I'd like to mention, is that in a backtracking algorithm without forward propagation, it is important to check if the solution is partially satisfied after assigning a value to a variable. However, because the forward propagation ensures that values in the domain are all valid assignments, checking for partial satisfaction is no longer required.

### Variable order heuristic

To pick the variable which is going to be set, a heuristic is used which will return the variable with the least amount of values in its domain, this is also called the minimum remaining values heuristic (MRV). This will ensure that the algorithm fails fast, and the search space is reduced quickly.

Some Sudoku's can be solved with only forward propagation. If we propagate a value, and a variable will be left with only one value in the domain, we could already set this value. However, this assignment then needs another propagation step, because a new value is set. Instead of assigning all the variables which just have a single value remaining in the domain, the recursive backtracking algorithm will do this step. This seemed the easiest choice. Because of the MRV heuristic, this variable will already be assigned immediately, giving the same effect as doing this in the forward propagation algorithm.

### Value order heuristic

Another common heuristic is to select the ordering of the values based on how many cells it will propagate to. In this case the value with the most propagated values will be picked. This gives more flexibility to the algorithm later on. However, I found that it increases the runtime, and even worse, it took the algorithm more assignments to solve the CSP. Instead of this, the values are picked in lexicographical order. The solver has an option available to change this heuristic.

## The hardest Sudoku

The hardest Sudoku is the following Sudoku string:

`800000000003600000070090200050007000000045700000100030001000068008500010090000400`

It takes the algoritm a bit less than 30 milliseconds, and 14.372 assigments, to solve this one.

```
--- Model ---
8........
..36.....
.7..9.2..
.5...7...
....457..
...1...3.
..1....68
..85...1.
.9....4..

--- Solution ---
812753649
943682175
675491283
154237896
369845721
287169534
521974368
438526917
796318452

State: Satisfied
Assigments: 14372
Time elapsed: 00:00:00.0279840 sec.
```

## Usage example with map coloring problem

The algorithm will solve any CSP model with only `AllDiff` constraints rather efficiently. It could be used for the N-queens problem, and also for the map coloring problem.

The example code below shows how to construct the map coloring problem.

```cs
var builder = new ModelBuilder();
var domain = model.CreateDomain("colors", Red, Green, Blue);
var wa = model.CreateVariable("Western Australia", domain);
var nt = model.CreateVariable("Northern Territory", domain);
var sa = model.CreateVariable("South Astralia", domain);
var qe = model.CreateVariable("Queensland", domain);
var nsw = model.CreateVariable("New South Wales", domain);
var vi = model.CreateVariable("Victoria", domain);
builder.CreateAllDifferentConstraint(wa, nt);
builder.CreateAllDifferentConstraint(wa, sa);
builder.CreateAllDifferentConstraint(nt, sa);
builder.CreateAllDifferentConstraint(nt, qe);
builder.CreateAllDifferentConstraint(sa, qe);
builder.CreateAllDifferentConstraint(sa, nsw);
builder.CreateAllDifferentConstraint(sa, vi);
builder.CreateAllDifferentConstraint(nsw, vi);
var model = builder.BuildCspModel();
```

The algorithm will solve this problem with just 6 assignments.

```
--- Model ---
Western Australia {0, 1, 2} = 0 (Set: False)
Northern Territory {0, 1, 2} = 0 (Set: False)
South Astralia {0, 1, 2} = 0 (Set: False)
Queensland {0, 1, 2} = 0 (Set: False)
New South Wales {0, 1, 2} = 0 (Set: False)
Victoria {0, 1, 2} = 0 (Set: False)

--- Solution ---
Western Australia {1, 2} = 0 (Set: True)
Northern Territory {2} = 1 (Set: True)
South Astralia {} = 2 (Set: True)
Queensland {} = 0 (Set: True)
New South Wales {1} = 0 (Set: True)
Victoria {} = 1 (Set: True)

State: Satisfied
Assignments: 6
Time elapsed: 00:00:00.0029048 sec.
```

## Update: feature model constraints

The following constraints have been added to the CSP solver.

 * `MandatoryConstraint`: parent and child must either both be on or off.
 * `OptionalConstraint`: if parent is on, child can be on or off. If the child is on, the parent must also be on.
* `AlternativeConstraint`: if the parent is on, one and only one child must be on. If a child is one, the parent must also be on.
* `OrConstraint`: if the parent is one, at least one child must be on. If a child is on, the parent must also be on.
* `RequireConstrant`: if the parent is on, the child must also be on. If the parent is off, the child can be either on or off.
* `ExcludeConstraint`: if the parent is on, the child must be off. If the child is on the parent must be off. The parent and child can not both be on.

As an example, a feature model for a phone (the iconic configurator hello world) is used. 

![Feature model for a phone](https://www.researchgate.net/profile/Mike_Papadakis/publication/263952669/figure/fig1/AS:296392913375233@1447676985293/A-simple-feature-model-of-a-mobile-phone-product-line-1-representing-the-features-and.png)

If this example is solved without partial assigment satisfaction and forward propagation, the result is rather slow. It takes a total of 662 assignments and around 800 milliseconds to solve the model. Note that the `Mobile Phone` variable must be enabled initially, otherwise it is smart enough to know that if everything is off the constraints are also satisfied. The time is also rather long because we are printing a warning to the debug console if a constraint doesn't have a propagator implemented.

```
--- Model ---
Mobile Phone {0, 1} = 1 (Set: True)
Calls {0, 1} = 0 (Set: False)
GPS {0, 1} = 0 (Set: False)
Screen {0, 1} = 0 (Set: False)
Basic {0, 1} = 0 (Set: False)
Colour {0, 1} = 0 (Set: False)
High resolution {0, 1} = 0 (Set: False)
Media {0, 1} = 0 (Set: False)
Camera {0, 1} = 0 (Set: False)
MP3 {0, 1} = 0 (Set: False)

--- Solution ---
Mobile Phone {0, 1} = 1 (Set: True)
Calls {0} = 1 (Set: True)
GPS {1} = 0 (Set: True)
Screen {0} = 1 (Set: True)
Basic {1} = 0 (Set: True)
Colour {1} = 0 (Set: True)
High resolution {0} = 1 (Set: True)
Media {1} = 0 (Set: True)
Camera {1} = 0 (Set: True)
MP3 {1} = 0 (Set: True)

State: Satisfied
Assignments: 662
Time elapsed: 00:00:00.8104605 sec.
```

Because partial satisfaction is not implemented, forward propagation is used. After implementing forward propagation for all the different constraints, the results are much better.

```
--- Model ---
Mobile Phone {0} = 1 (Set: True)
Calls {0, 1} = 0 (Set: False)
GPS {0, 1} = 0 (Set: False)
Screen {0, 1} = 0 (Set: False)
Basic {0, 1} = 0 (Set: False)
Colour {0, 1} = 0 (Set: False)
High resolution {0, 1} = 0 (Set: False)
Media {0, 1} = 0 (Set: False)
Camera {0, 1} = 0 (Set: False)
MP3 {0, 1} = 0 (Set: False)

--- Solution ---
Mobile Phone {0} = 1 (Set: True)
Calls {} = 1 (Set: True)
GPS {1} = 0 (Set: True)
Screen {} = 1 (Set: True)
Basic {1} = 0 (Set: True)
Colour {1} = 0 (Set: True)
High resolution {0} = 1 (Set: True)
Media {1} = 0 (Set: True)
Camera {1} = 0 (Set: True)
MP3 {1} = 0 (Set: True)

State: Satisfied
Assignments: 24
Time elapsed: 00:00:00.0024186 sec.
```

In this case it only takes 24 assignments and a total of 2 milliseconds.

## Update: solving 50 Sudoku's for Project Euler

[Project Euler has a problem which requires you to solve 50 Sudoku puzzles](https://projecteuler.net/problem=96), and take the sum of the first three digit number in the solution. The three digit number is the number `812` in the example below.

```
812753649
943682175
675491283
154237896
369845721
287169534
521974368
438526917
796318452
```

To do this, we will first read in the [Sudoku puzzles from a text file](https://projecteuler.net/project/resources/p096_sudoku.txt), and parse each puzzle into a single string, which is then used as input for the Sudoku model builder.

```cs
public static IEnumerable<string> PuzzleIterator()
{
    var lines = File.ReadAllLines("puzzles.txt");
    for(int i = 1; i < lines.Length; i += 10)            
        yield return string.Join("", lines[i..(i+9)]);
}
```

We can then use this function to generate all the CSP models, and solve them. After the puzzle is found, we calculate the three digit number from the solution and add it to the total sum.

```cs
public static void SolveProjectEulerPuzzle()
{
    var threeDigitSum = 0;
    var stopwatch = new Stopwatch();
    stopwatch.Start();
    foreach(var puzzle in PuzzleIterator())
    {
        var builder = SudokuModel.GetModel(puzzle);
        var model = builder.BuildCspModel();
        var solver = new BacktrackSearcher(model);
        var state = solver.Solve();
        int threeDigitNumber = model.Variables[0].Value * 100 
                             + model.Variables[1].Value * 10 
                             + model.Variables[2].Value;
        threeDigitSum += threeDigitNumber;
    }
    stopwatch.Stop();
    Console.WriteLine($"Solution: {threeDigitSum}");
    Console.WriteLine($"Total runtime: {stopwatch.Elapsed} sec.");
}
```

If we run this method, we will find that the solution is `24702` in around 56 milliseconds, not bad.

```
Solution: 24702
Total runtime: 00:00:00.0561051 sec.
```
