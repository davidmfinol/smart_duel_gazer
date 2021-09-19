# Tests

All logic inside of C# classes should be unit tested, apart from MonoBehaviours, SciptableObjects and other Unity classes. If there's a lot of logic inside of Unity classes, it should be moved somewhere else so it can be tested. If your code isn't testable, it isn't great.

We use [moq][moq] for mocking our dependencies and verifying interations on them.

We follow the [given-when-then][given-when-then] principle for writing tests.

Some example unit tests can be found in ConnectionDataManagerTests.cs.

[moq]: https://github.com/Moq/moq4/wiki/Quickstart
[given-when-then]: https://martinfowler.com/bliki/GivenWhenThen.html
