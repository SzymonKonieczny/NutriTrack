import sqlite3

# Path to your SQLite database file
DB_PATH = "NutriTrack.db"

# List of target micronutrient IDs
TARGET_IDS = [
    "D4303A4A-A4AE-4E90-B0C2-497DBBF341C2",
"5DBD4436-8961-4CB2-99CA-54127D6BA7F7",
"ACD3B43E-5B9C-44C1-B96D-25B7B725F30B",
"9354466C-D2EF-41D2-AA33-26A2053F2C3D"
]  # Replace with your actual IDs


def scale_micronutrients(db_path, ids_to_update):
    if not ids_to_update:
        print("No IDs provided. Exiting.")
        return

    # Create placeholders for the SQL IN clause (?, ?, ?)
    placeholders = ",".join(["?"] * len(ids_to_update))

    # SQL query to divide AmountPer100g by 1000 for matching rows
    sql_query = f"""
        UPDATE IngredientMicronutrients
        SET AmountPer100g = AmountPer100g / 1000.0
        WHERE micronutrientId IN ({placeholders});
    """

    try:

        print(
            f"Stariting."
        )
        conn = sqlite3.connect(db_path)
        cursor = conn.cursor()

        # Execute the update query
        cursor.execute(sql_query, ids_to_update)

        # Get the number of rows affected
        updated_rows = cursor.rowcount

        # Commit changes to the database
        conn.commit()

        print(
            f"Successfully updated {updated_rows} row(s) in 'IngredientMicronutrient'."
        )

    except sqlite3.Error as e:
        print(f"An error occurred: {e}")
        if conn:
            conn.rollback()
    finally:
        if conn:
            conn.close()


if __name__ == "__main__":
    scale_micronutrients(DB_PATH, TARGET_IDS)