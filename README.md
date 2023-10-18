# GitStractor - Git Repository Analysis Tool
Project by [Matt Eland](https://LinkedIn.com/in/matteland) ([@IntegerMan](https://twitter.com/IntegerMan))

This project is built for extracting commit, author, and file data from local git repositories in order to visualize repository history and trends to provide insight to software development teams and their stakeholders.

Here are a few examples of the types of visualizations that can be generated using GitStractor:

Stacked bar chart of # of commits per month by day of week:
![Accessible AI Blog Posts by Month](./Images/AccessibleAIBlogPostsByMonth.png)

Tree map of the # of commits per file:
![GitStractor # Commits by File](./Images/GitStractorFileCommits_April_1_2023.png)

Scatter plot of files in each commit:
![GitStractor # Files per Commit](./Images/GitStractorFilesPerCommit_April_1_2023.png)

## Disclaimer

This project is in technical preview and will have bugs, inaccuracies, and numerous other issues. Use at your own risk.

Additionally, this project is intended to help you get a sense of the overall trends in your source code. It should not be used for performance evaluation purposes as its data is not yet known to be reliable and git history is not a good indicator of an indivual's performance.

## Usage Instructions

GitStractor currently has two components:
- A command line tool for extracting data from a local git repository
- A Jupyter notebook for visualizing the extracted data

Data is extracted using the **GitStractor-Extract** tool. You can build this by opening `GitStractor\GitStractor.sln` in [Visual Studio](https://visualstudio.microsoft.com/) and setting `GitStractor-Extract` as the startup project.
See **Extracting Commit Data** below for more information.

Once data is extracted, you can visualize it in a [Jupyter Notebook](Notebooks/GitStractor.ipynb).
See **Visualizing Commit Data in Jupyter Notebooks** below for more information.

### Extracting Commit Data

To get started, you'll need to run the `GitStractor` project in the `GitStractor\GitStractor.sln` solution.

You can either run the program from the command line once you've built it, or you can customize `launchSettings.json` to meet your needs.

Some common usage scenarios:

Extract git information from the git repository at C:\Dev\Interactive and store the resulting CSV files in C:\GitStractor
```
GitStractor-Extract --source C:\Dev\Interactive --destination C:\GitStractor
```

Extract git information from the git repository at C:\Dev\Interactive and store the resulting CSV files in C:\GitStractor. 
Ignore .gif, .txt, .json, and .d.ts files.
```
GitStractor-Extract -s C:\Dev\Interactive -d C:\GitStractor --ignore .gif,.txt,.json,.d.ts
```

Extract git information from the git repository at C:\Dev\Interactive and store the resulting CSV files in C:\GitStractor. 
Do not analyze commits off the current branch. This will analyze merge commits and may misattribute commits to the user that merged them. It is also considerably faster.
```
GitStractor-Extract -s C:\Dev\Interactive -d C:\GitStractor --includebranches false
```

Extract git information from the git repository at C:\Dev\Interactive and store the resulting CSV files in C:\GitStractor. 
Uses an authormap.json file to rename or merge together users. This is handy when you have users that have committed under different E-Mail addresses or changed their names.
```
GitStractor-Extract -s C:\Dev\Interactive -d C:\GitStractor --authormap C:\Dev\AuthorMap.json
```

An `AuthorMap.json` file should be structured like this:

```json
[
    {
        "name": "Matt Eland",
        "emails": ["meland@gitstractor.com", "matt@gitstractor.com"]
    },
    {
        "name": "GitStractor",
        "emails": ["noreply@gitstractor.com"],
        "bot": true
    }
]
```

### Visualizing Commit Data in Jupyter Notebooks

To view the full range of data visualizations in Jupyter Notebooks, open the `GitStractor.ipynb` Jupyter notebook in the `Notebooks` folder. 

Once there, change the `project_name` variable to reflect your project and change the `data_dir` to indicate the directory your GitStractor .csv files from the prievious step are located.

From there, run the notebook from top to bottom to generate recommendations.

In order to work with Jupyter Notebooks, I recommend you install [Anaconda](https://www.anaconda.com/download/) and [VS Code](https://code.visualstudio.com/Download).

## What's Next?

Future efforts on this project will focus on:

- Creating a desktop application for extracting and visualizing data
- Expanding the range of visualizations available in the Jupyter notebook
- Improving the user experience pulling data from larger repositories
- Adding machine learning capabilities for commit classification and clustering

If you'd like to submit a feature request or view the current backlog, please visit the [GitHub Issues tab](https://github.com/IntegerMan/gitstractor/issues)

## Contact

Contact [Matt Eland](https://MattEland.dev) for general questions and feedback.

Please open an issue for enhancement requests and bug reports.
