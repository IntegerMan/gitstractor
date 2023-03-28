# GitStractor Python Prototype

This directory represents the old and obsolete prototype of GitStractor done in Python. It is useful for anyone curious about git analysis from Python, but has some fundamental performance flaws and known issues.

Once the dotnet version is ready, that will be the preferred version of GitStractor.

## Known Issues

This project is a rough prototype at present and may have blatant bugs and performance issues.

Current known bugs:

- Performance is very slow to analyze repositories and ranges from 0.2 to 0.8 seconds per commit
- Moved or renamed files are not accurately tracked in the final results

## Python Prototype

The current working edition of this project involves a Jupyter Notebook that uses PyDriller to mine git repositories and generate CSV files that can be imported into Tableau for analysis.

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
   4. `MergedFileData.csv` which joins together `FileCommits.csv` and `FileSizes.csv` to provide file information at a per-file-commit basis
   5. `FileData.csv` which joins together `FileSizes.csv` and `FileCommits.csv` to provide commit aggregate information at a per-file basis

The data should now be ready to import into Tableau, Power BI, or another tool. You can also analyze the data in Python or another programming language

### Visualization

Currently, the data is available for viewing in Tableau if you refresh the data sources. Find the `GitStractor.twb` file and ensure the data source is properly connected to the local CSV files.

