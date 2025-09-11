import xml.etree.ElementTree as ET
import os
import csv

# Folder containing the XML files
folder_path = './'  # replace with your folder path if needed

# Output CSV file
output_file = 'questions.csv'

# Namespace from QTI XML
ns = {'qti': 'http://www.imsglobal.org/xsd/imsqti_v2p1'}

all_questions = []

# Loop through all XML files in the folder
for filename in os.listdir(folder_path):
    if filename.endswith('.xml'):
        file_path = os.path.join(folder_path, filename)
        try:
            tree = ET.parse(file_path)
            root = tree.getroot()

            # Find correct response
            correct_value_elem = root.find('.//qti:correctResponse/qti:value', ns)
            correct_id = correct_value_elem.text if correct_value_elem is not None else ''

            for item in root.findall('.//qti:itemBody', ns):
                # Get question text safely
                div = item.find('.//qti:div', ns)
                if div is not None:
                    p = div.find('p')
                    question_text = p.text.strip() if p is not None and p.text else '—'
                else:
                    question_text = '—'

                # Get choices
                choices = item.findall('.//qti:simpleChoice', ns)
                option_texts = []
                correct_option = ''

                for i, choice in enumerate(choices):
                    text = choice.text.strip() if choice.text else '—'
                    option_texts.append(text)
                    if choice.attrib.get('identifier') == correct_id:
                        correct_option = chr(65 + i)  # A, B, C, D

                # Pad options to 4
                while len(option_texts) < 4:
                    option_texts.append('—')

                all_questions.append([question_text] + option_texts + [correct_option])
        except ET.ParseError:
            print(f"Error parsing {filename}, skipping this file.")

# Write to CSV (tab-separated) with UTF-8 BOM for Excel compatibility
with open(output_file, 'w', newline='', encoding='utf-8-sig') as f:
    writer = csv.writer(f, delimiter='\t')
    # Header
    writer.writerow(['Id', 'Question', 'OptionA', 'OptionB', 'OptionC', 'OptionD', 'CorrectOption'])
    # Write questions with Id
    for idx, q in enumerate(all_questions, start=1):
        writer.writerow([idx] + q)

print(f"Conversion complete! {len(all_questions)} questions saved to {output_file}")
