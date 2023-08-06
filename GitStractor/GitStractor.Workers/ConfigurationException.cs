﻿using CommandLine;

namespace GitStractor.Workers; 

public class ConfigurationException : Exception {

    public ConfigurationException(string message) : this(message, Enumerable.Empty<Error>()) {
    }

    public ConfigurationException(string message, IEnumerable<Error> errors) : base(message) {
        Errors = errors;
    }

    public IEnumerable<Error> Errors { get; }
}