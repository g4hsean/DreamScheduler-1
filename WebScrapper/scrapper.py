#!/usr/bin/python
from bs4 import BeautifulSoup
import requests
import sys

def getSeptEntry():
    page = requests.get('http://www.concordia.ca/encs/computer-science-software-engineering/students/course-sequences/sept-soen-general.html')
    response = page.text
    removeListRows = ['Year','<th>', 'Â']
    
    htmlEntities = BeautifulSoup(response)
    generalCourses = htmlEntities.find_all("div", {"class" : "accordion-panel section"})
    output = '{ "generalClasses": ['
    
    for entry in generalCourses:
        table = entry.findAll("table")
        rows = table[0].findAll("tr")[2:]
        temp = ''
        
        for row in rows:
            if not any(entry in str(row) for entry in removeListRows):
               #temp.append(row)
                td = row.findAll('td')
                output += '["' + str(td[0].p.text) + '", "' + str(td[1].p.text) + '", "' + str(td[2].p.text).replace("(","").replace(")","") + '"], \n'
    output = output[:-3] + "]\n}"
    print output

getSeptEntry()
