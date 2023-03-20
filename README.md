# GitStractor
Project by [Matt Eland](https://LinkedIn.com/in/matteland) ([@IntegerMan](https://twitter.com/IntegerMan))

This project is built for Extracting data from local git repositories and their file trees in order to visualize repository information to provide insight to teams.

## Known Issues

This project is a rough prototype at present and may have blatant bugs and performance issues.

Current known bugs:

- Performance is very slow to analyze repositories and ranges from 0.2 to 0.8 seconds per commit
- Moved or renamed files are not accurately tracked in the final results

## Python Prototype

### File Analysis Workflow

1. Clone your repository locally using `git clone` or a Git tool such as GitKraken or GitHub Desktop.
2. Install all requirements needed:
   1. Pandas
   2. PyDriller
3. Open `GitStractor.ipynb`
4. Set `repository_path` equal to the local file path of your git repository. You do not need to specify `.git`, just the local folder. For example: `repository_path = 'C:\\dev\\GitStractor'`
5. Optionally set the `repository_branch` if you only want to analyze the main branch (this is recommended for performance and clarity of results)
6. Run all cells in `Gather.ipynb` this will generate:
   1. `Commits.csv` containing all git commits
   2. `FileCommits.csv` which breaks down commits at a one row per file per commit level
   3. `FileSizes.csv` containing file statistics for all source files in the current version of your project
   4. `MergedFileData.csv` which joins together `FileCommits.csv` and `FileSizes.csv`

The data should now be ready to import into Tableau, Power BI, or another tool. You can also analyze the data in Python or another programming language

### Visualization

Currently, the data is available for viewing in Tableau if you refresh the data sources. Find the `GitAnalysis.twbx` file and refresh the data source after visualizing your code.

## Contact Information

Contact [Matt Eland](https://MattEland.dev) for general questions and feedback.

Please open an issue for enhancement requests and bug reports.
