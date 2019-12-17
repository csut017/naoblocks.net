# Projects

This page lists the projects in NaoBlocks.Net and their functionality.

## NaoBlocks.Core

This project contains the command framework (including commands) and data models. It is designed to be independent of any user interface, allowing the potential (in future) to design different front-ends.

### NaoBlocks.Core.Tests

Unit tests for NaoBlocks.Core.

## NaoBlocks.Parser

This project contains the compiler for the internal DSL used for communications (see [NaoBlocks: A Case Study of Developing a Children's Robot Programming Environment](https://www.doi.org/10.1109/URAI.2018.8441843)). The DSL is based on RoboLang (see [RoboLang: A Simple Domain Specific Language to Script Robot Interactions](https://www.doi.org/10.1109/URAI.2019.8768625)) but with some customisations to provide additional functionality needed for NaoBlocks and a different set of built-in functions.

### NaoBlocks.Parser.Tests

Unit tests for NaoBlocks.Parser.

## NaoBlocks.Web

A web front-end for NaoBlocks.Net. 

### NaoBlocks.Web.Tests

Unit tests for NaoBlocks.Web.