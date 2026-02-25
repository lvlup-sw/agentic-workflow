// =============================================================================
// <copyright file="SpecialistPersona.cs" company="Levelup Software">
// Copyright (c) Levelup Software. All rights reserved.
// </copyright>
// =============================================================================

namespace Strategos.Agents.Models;

/// <summary>
/// Defines the personality and behavior configuration for a specialist agent.
/// </summary>
/// <remarks>
/// <para>
/// A persona encapsulates everything that differentiates one specialist from another:
/// </para>
/// <list type="bullet">
///   <item><description>The specialist type identifier</description></item>
///   <item><description>The system prompt that guides code generation</description></item>
///   <item><description>Optional confidence level for successful executions</description></item>
/// </list>
/// <para>
/// This enables composition-based specialist creation without requiring separate classes
/// for each specialist type.
/// </para>
/// </remarks>
/// <param name="Type">The specialist type this persona represents.</param>
/// <param name="SystemPrompt">The system prompt used during code generation.</param>
/// <param name="SuccessConfidence">The confidence score for successful executions (default 0.9).</param>
public sealed record SpecialistPersona(
    SpecialistType Type,
    string SystemPrompt,
    double SuccessConfidence = 0.9)
{
    /// <summary>
    /// Validates the persona configuration.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when SystemPrompt is null or whitespace, or SuccessConfidence is outside [0, 1].
    /// </exception>
    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(SystemPrompt, nameof(SystemPrompt));

        if (SuccessConfidence is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(SuccessConfidence),
                SuccessConfidence,
                "Success confidence must be between 0 and 1");
        }
    }

    /// <summary>
    /// Gets the predefined Coder persona.
    /// </summary>
    public static SpecialistPersona Coder => new(
        SpecialistType.Coder,
        """
        You are a Python coder. When asked to perform tasks, write Python code to accomplish them.

        Rules:
        1. Output ONLY the Python code, no explanations or markdown formatting
        2. Use print() to output results that should be returned to the user
        3. Handle potential errors gracefully with try/except blocks
        4. Keep code concise and efficient
        5. Only import standard library modules unless specifically asked otherwise
        6. Do NOT use markdown code blocks (```python) - output raw Python code only

        Example:
        User: Calculate 2 + 2
        You: print(2 + 2)

        Example:
        User: Find the factorial of 10
        You: import math
        print(math.factorial(10))
        """);

    /// <summary>
    /// Gets the predefined Analyst persona.
    /// </summary>
    public static SpecialistPersona Analyst => new(
        SpecialistType.Analyst,
        """
        You are a data analyst. When asked to perform analysis tasks, write Python code to accomplish them.

        Rules:
        1. Output ONLY the Python code, no explanations or markdown formatting
        2. Use pandas, numpy, and statistics libraries for data analysis
        3. Use print() to output results that should be returned to the user
        4. Handle potential errors gracefully with try/except blocks
        5. Keep code concise and efficient
        6. Do NOT use markdown code blocks (```python) - output raw Python code only

        Common operations:
        - Statistical analysis: mean, median, std, variance, correlation
        - Data transformations: filtering, grouping, aggregation
        - Calculations: sums, percentages, ratios
        - Visualization: matplotlib plots (save to file)

        Example:
        User: Calculate the mean of [1, 2, 3, 4, 5]
        You: import numpy as np
        data = [1, 2, 3, 4, 5]
        print(np.mean(data))

        Example:
        User: Find the standard deviation of sales data
        You: import pandas as pd
        import numpy as np
        sales = pd.Series([100, 150, 200, 175, 225])
        print(f"Standard Deviation: {np.std(sales):.2f}")
        """);
}
