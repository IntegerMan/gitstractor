import pandas as pd

def extract_file_information(row, df_commits=None):
    path = row['relative_path']

    initial_author = 'Unknown'
    latest_author = 'Unknown'
    num_commits = 1
    num_inserts = 0
    num_deletes = 0
    frequent_author = 'Unknown'
    num_authors = 1
    initial_commit = None
    latest_commit = None

    try:
        matching = df_commits.query('new_path == "' + path + '"')

        # Extract author information
        authors = matching['author_name']
        initial_author = authors.iat[0]
        latest_author = authors.iat[len(authors) - 1]
        frequent_authors = authors.mode()
        frequent_author = frequent_authors.iat[0]
        num_authors = len(authors.distinct())

        # Extract date information
        dates = matching['author_date']
        initial_commit = dates.iat[0]
        latest_commit = dates.iat[len(dates) - 1]

        # Extract commit information
        num_commits = len(authors)
        num_inserts = matching['num_inserts'].sum()
        num_deletes = matching['num_deletes'].sum()
    except:
        # Sometimes information cannot be found about files due to renames or missing history
        pass

    # Add new values to row
    row['Initial Author'] = initial_author
    row['Latest Author'] = latest_author
    row['Most Frequent Author'] = frequent_author
    row['Unique Authors'] = num_authors
    row['Initial Commit'] = initial_commit
    row['Latest Commit'] = latest_commit
    row['Total Commits'] = num_commits
    row['Total Inserts'] = num_inserts
    row['Total Deletes'] = num_deletes

    return row


def generate_merged_file_data(file_commits_path = 'FileCommits.csv', 
                              file_size_data_path='FileSizes.csv', 
                              output_file = 'MergedFileData.csv',
                              file_data_path = 'FileData.csv'):
    """
    Joins together file commit data and file size data into a pair of denormalized data files.
    The first focuses on the commits and file information alongside those commits.
    The second focuses on the files and rolls commit information into those commits.
    """
    print('Loading file commit data from ' + file_commits_path)
    df_file_commits = pd.read_csv(file_commits_path)

    # Fix the junk column to be an ID
    df_file_commits.rename(columns={'Unnamed: 0': 'File_Commit_ID'}, inplace=True)

    # Because our test data was cloned to a temp directory as part of PyDriller, let's substitute it with the correct local path
    df_file_commits['relative_path'] = df_file_commits['new_path']

    # Replace NaN values with '' for readability
    df_file_commits.fillna('', inplace=True)

    print('Loading file size data from ' + file_size_data_path)
    df_files = pd.read_csv(file_size_data_path)

    # Pandas guesses at ID column names. Make the name make sense
    df_files.rename(columns={'Unnamed: 0': 'File_ID'}, inplace=True)

    # Replace '.' values (root directory) with '' instead
    df_files.replace({'path': {'.': ''}}, inplace=True)

    # These columns provide no additional information and muddy comparisons later
    df_files.drop(columns=['root', 'fullpath'], inplace=True) 

    # Create a new data frame by joining together the other two on their relative paths
    df_merged = pd.merge(df_file_commits, df_files, left_on='relative_path', right_on='relative_path')

    # Remove not needed and consolidate duplicated columns
    df_merged.drop(columns=['File_Commit_ID', 'File_ID', 'filename_x'], inplace=True)
    df_merged.rename(columns={'filename_y': 'filename'}, inplace=True)

    # Save the resulting dataset to disk
    df_merged.to_csv(output_file)
    print('Merged file data created in ' + output_file)

    # Load commits
    df_file_commits.sort_values(by=['author_date'], ascending=True, inplace=True)

    # Extract additional insights
    print('Aggregating file data')
    df_files = df_files.apply(lambda path: extract_file_information(path, df_file_commits), axis=1)

    # Write the file to disk
    print('Writing file data to ' + file_data_path)    
    df_files.to_csv(file_data_path)
    print('File generation completed')
    