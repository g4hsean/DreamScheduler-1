#!/usr/bin/python
# -*- coding: utf-8 -*-

from bs4 import BeautifulSoup
import requests
import sys
import re
import pprint
import json
from unidecode import unidecode
import os

def convertTimeFormat(dateTime):
    dateConverter = {'M' : 'Monday', 'T' : 'Tuesday', 'W': 'Wednesday', 'J' : 'Thursday', 'F': 'Friday', 'S' : 'Saturday', 'D' : 'Sunday'} # used to convert M--J--- to a list of Monday, Thursday
    convertedTimes = {'Days' : [], 'Start-Time' : '', 'End-Time' : ''}
    
    timeObject = dateTime.strip().split(" ")
    for day in timeObject[0]:
        if day in dateConverter.keys():
            convertedTimes['Days'].append(dateConverter[day])

    extractedTime = timeObject[1].replace('(','').replace(')','').split('-')
    convertedTimes['Start-Time'] = extractedTime[0]
    convertedTimes['End-Time'] = extractedTime[1]

    return convertedTimes
    
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

def getFallWinter(sequenceDict):
    courseInformationMasterList = {}
    
    #required post variables for session list retrieval.
    eventValidation = '/wEWhwECi5Sl6QYC1K/Vsw0C1K/hiAoCuLveuQMCubveuQMCu7veuQMCurveuQMC6LveuQMChvmOqwgCtPmOqwgCsvmOqwgC14/UggkC0bPysw0C9tz4+Q8C9tyQjw0C7c7n6ggC7c6/GQLtzofDAgK8idTaAQK8ieC1DgLtzsv1CALtztfQAQL23ITUBAK8iYyQBwLtzuOLDgKVhPTtAwLtzo/mBgKVhIDICAKLxea/DgL23NShCwKC99XzDwL23OAcAryJmMsPAryJpKYEAryJ8M8BApWE6LILApWExPoGApWEuJ4OApWE0NUPAryJnKoOAoL3jYkNAteP7IIJAu3Oq74LAu3O/+oIAu3Ow/UIAu3O8+gFAoL37fMPAu3OtxkC7c7v0AEC7c77iw4C14/oggkC7c6D5gYC7c7vDwLtzuvQAQLtzvvqCALtzo/oBQLtzrMZAu3O3/UIAteP4IIJAoL39fEIAu3OyxoC7c7X9QgCgvfh8w8CgvfBmgoCgveNrgQC7c6H6AUC7c7j0AEC7c7z6ggC7c7nDwLXj7SBCQLtztvoBQKbtugpApu2+MgOAvXPtacDAtvurucOArXj1tEPAtqLiocLArXj7t4PAtqLooQLAtzj1tEPAtzj7t4PAsGIooQLArXjiqEOAtzjiqEOAsGI5tUFAt+fnaoNAsacnaoNAt+f0fsPAsac0fsPAsac6fgPArXjus0MAtzjus0MArXj/poPAtzj/poPAt+fgYYCAsacgYYCAt+fxdcMAsacxdcMAtX61tEPArz61tEPAtX6iqEOArz6iqEOAqGj5tUFAv+3naoNAqa3naoNAv+30fsPAqa30fsPAouRsNEMAtX6us0MAtX6/poPArz6/poPAv+3gYYCAv+3xdcMAqyOjMwNAqa3xdcMAouRjMwNAqa3jdcMAouR1M0NAuezssQJAoSzssQJAouzssQJAo6zssQJAoGzssQJAr2zssQJAoqzssQJAruzssQJAonkuM8PAprElP0FArX3q6sKAr+cuY0NAsHjxcUKAqXjxcUKAqzjxcUKAtLvmtQCAtzW0NEHHJuFUolpqP9xRO40rxwwf9+GmrM='
    viewState = '/wEPDwULLTE5NzE0Mjg1OTMPZBYCZg9kFgQCAw9kFgJmD2QWAgIBD2QWAgICDw8WAh4EVGV4dAUJMjAxNC0yMDE1ZGQCBQ9kFgICAQ9kFgQCAQ9kFgJmD2QWAmYPZBYCZg9kFgICAQ9kFgYCAQ9kFgICAg9kFgJmDxAPFgYeDURhdGFUZXh0RmllbGQFBWRlc2NyHg5EYXRhVmFsdWVGaWVsZAUJQ09ERVZBTFVFHgtfIURhdGFCb3VuZGdkEBUCBzIwMTQtMTUHMjAxMy0xNBUCBDIwMTQEMjAxMxQrAwJnZxYBZmQCAw9kFgICAg9kFgJmDxAPFgofAQUIRkFDX0RFUFQfAgUJQ09ERVZBTFVFHwNnHglCYWNrQ29sb3IKpAEeBF8hU0ICCGQQFTwOQVJUUyAmIFNDSUVOQ0UYLSBBcHBsaWVkIEh1bWFuIFNjaWVuY2VzCS0gQmlvbG9neRwtIENoZW1pc3RyeSBhbmQgQmlvY2hlbWlzdHJ5LC0gQ2xhc3NpY3MsIE1vZGVybiBMYW5ndWFnZXMgYW5kIExpbmd1aXN0aWNzFy0gQ29tbXVuaWNhdGlvbiBTdHVkaWVzIS0gRGVhbiBvZiBBcnRzICYgU2NpZW5jZSAoT2ZmaWNlKQstIEVjb25vbWljcwstIEVkdWNhdGlvbgktIEVuZ2xpc2gTLSBFdHVkZXMgRnJhbmNhaXNlcxItIEV4ZXJjaXNlIFNjaWVuY2UlLSBHZW9ncmFwaHksIFBsYW5uaW5nIGFuZCBFbnZpcm9ubWVudAktIEhpc3RvcnkbLSBJbnRlcmRpc2NpcGxpbmFyeSBTdHVkaWVzDC0gSm91cm5hbGlzbRYtIExpYmVyYWwgQXJ0cyBDb2xsZWdlHi0gTG95b2xhIEludGVybmF0aW9uYWwgQ29sbGVnZRwtIE1hdGhlbWF0aWNzIGFuZCBTdGF0aXN0aWNzDC0gUGhpbG9zb3BoeQktIFBoeXNpY3MTLSBQb2xpdGljYWwgU2NpZW5jZQwtIFBzeWNob2xvZ3kKLSBSZWxpZ2lvbiItIFNjaG9vbCBvZiBDYW5hZGlhbiBJcmlzaCBTdHVkaWVzKC0gU2Nob29sIG9mIENvbW11bml0eSBhbmQgUHVibGljIEFmZmFpcnMRLSBTY2llbmNlIENvbGxlZ2UvLSBTaW1vbmUgZGUgQmVhdXZvaXIgSW5zdGl0dXRlICYgV29tZW5zIFN0dWRpZXMcLSBTb2Npb2xvZ3kgYW5kIEFudGhyb3BvbG9neRUtIFRoZW9sb2dpY2FsIFN0dWRpZXMeSk9ITiBNT0xTT04gU0NIT09MIE9GIEJVU0lORVNTDS0gQWNjb3VudGFuY3kPLSBFeGVjdXRpdmUgTUJBCS0gRmluYW5jZRgtIEdlbmVyYWwgQWRtaW5pc3RyYXRpb24sLSBHb29kbWFuIEluc3RpdHV0ZSBpbiBJbnZlc3RtZW50IE1hbmFnZW1lbnQMLSBNYW5hZ2VtZW50Cy0gTWFya2V0aW5nMS0gU3VwcGx5IENoYWluIGFuZCBCdXNpbmVzcyBUZWNobm9sb2d5IE1hbmFnZW1lbnQeRU5HSU5FRVJJTkcgJiBDT01QVVRFUiBTQ0lFTkNFMC0gQnVpbGRpbmcsIENpdmlsLCBhbmQgRW52aXJvbm1lbnRhbCBFbmdpbmVlcmluZyMtIENlbnRyZSBmb3IgRW5naW5lZXJpbmcgaW4gU29jaWV0eSstIENvbXB1dGVyIFNjaWVuY2UgYW5kIFNvZnR3YXJlIEVuZ2luZWVyaW5nOS0gQ29uY29yZGlhIEluc3RpdHV0ZSBmb3IgSW5mb3JtYXRpb24gU3lzdGVtcyBFbmdpbmVlcmluZxUtIERlYW4gb2YgRW5naW5lZXJpbmclLSBFbGVjdHJpY2FsIGFuZCBDb21wdXRlciBFbmdpbmVlcmluZyctIE1lY2hhbmljYWwgYW5kIEluZHVzdHJpYWwgRW5naW5lZXJpbmcJRklORSBBUlRTDy0gQXJ0IEVkdWNhdGlvbg0tIEFydCBIaXN0b3J5CC0gQ2luZW1hFC0gQ29udGVtcG9yYXJ5IERhbmNlGS0gQ3JlYXRpdmUgQXJ0cyBUaGVyYXBpZXMdLSBEZXNpZ24gYW5kIENvbXB1dGF0aW9uIEFydHMLLSBGaW5lIEFydHMHLSBNdXNpYw0tIFN0dWRpbyBBcnRzCS0gVGhlYXRyZRtTQ0hPT0wgT0YgRVhURU5ERUQgTEVBUk5JTkcdLSBTY2hvb2wgb2YgRXh0ZW5kZWQgTGVhcm5pbmcVPAIwMQQwMTQwBDAxNTEEMDE1MwQwMTA5BDAxMDMEMDEwMQQwMTMzBDAxMzQEMDEwNAQwMTA1BDAxNTIEMDEzNQQwMTA2BDAxODEEMDEwNwQwMTgyBDAxNjIEMDE1NgQwMTEwBDAxNTcEMDEzNgQwMTM3BDAxMzgEMDE4MAQwMTg1BDAxODQEMDE4NgQwMTM5BDAxMTICMDMEMDMwMgQwMzA5BDAzMDQEMDMwMAQwMzEwBDAzMDMEMDMwNQQwMzA2AjA0BDA0MDcEMDQwOAQwNDA1BDA0MDkEMDQwMAQwNDAzBDA0MDQCMDYEMDYxNwQwNjAzBDA2MDQEMDYxMAQwNjE4BDA2MTEEMDYwMAQwNjA1BDA2MDkEMDYwOAIwOQQwOTAwFCsDPGdnZ2dnZ2dnZ2dnZ2dnZ2dnZ2dnZ2dnZ2dnZ2dnZ2dnZ2dnZ2dnZ2dnZ2dnZ2dnZ2dnZ2dnZ2dnZ2dnZ2RkAgkPZBYCAgEPZBYCZg9kFgICAg9kFgRmD2QWAmYPEA8WBh8BBQlDTEFTU0RBWVMfAgUJQ0xBU1NEQVlTHwNnZBAVLAEgBy0tLS0tLS0HLS0tLS0tRActLS0tLVMtBy0tLS0tU0QHLS0tLUYtLQctLS0tRlMtBy0tLS1GU0QHLS0tSi0tLQctLS1KRi0tBy0tLUpGU0QHLS1XLS0tLQctLVctRi0tBy0tV0otLS0HLS1XSkYtLQctLVdKRlMtBy1ULS0tLS0HLVQtLUYtLQctVC1KLS0tBy1ULUpGLS0HLVRXLS0tLQctVFctRi0tBy1UV0otLS0HLVRXSkYtLQdNLS0tLS0tB00tLS1GLS0HTS0tSi0tLQdNLS1KRi0tB00tLUpGU0QHTS1XLS0tLQdNLVctRi0tB00tV0otLS0HTS1XSkYtLQdNLVdKRlNEB01ULS0tLS0HTVQtSi0tLQdNVC1KRi0tB01UVy0tLS0HTVRXSi0tLQdNVFdKLS1EB01UV0pGLS0HTVRXSkYtRAdNVFdKRlMtB01UV0pGU0QVLAEgBy0tLS0tLS0HLS0tLS0tRActLS0tLVMtBy0tLS0tU0QHLS0tLUYtLQctLS0tRlMtBy0tLS1GU0QHLS0tSi0tLQctLS1KRi0tBy0tLUpGU0QHLS1XLS0tLQctLVctRi0tBy0tV0otLS0HLS1XSkYtLQctLVdKRlMtBy1ULS0tLS0HLVQtLUYtLQctVC1KLS0tBy1ULUpGLS0HLVRXLS0tLQctVFctRi0tBy1UV0otLS0HLVRXSkYtLQdNLS0tLS0tB00tLS1GLS0HTS0tSi0tLQdNLS1KRi0tB00tLUpGU0QHTS1XLS0tLQdNLVctRi0tB00tV0otLS0HTS1XSkYtLQdNLVdKRlNEB01ULS0tLS0HTVQtSi0tLQdNVC1KRi0tB01UVy0tLS0HTVRXSi0tLQdNVFdKLS1EB01UV0pGLS0HTVRXSkYtRAdNVFdKRlMtB01UV0pGU0QUKwMsZ2dnZ2dnZ2dnZ2dnZ2dnZ2dnZ2dnZ2dnZ2dnZ2dnZ2dnZ2dnZ2dnZ2dnZ2cWAWZkAgEPZBYCZg8QZGQWAWZkAgMPDxYCHgdWaXNpYmxlaGRkZJTK9VTOWW8tCPl00UdYB+JgPGvE'
    postVariables = {'__EVENTTARGET':'ctl00$PageBody$btn_ShowScCrs','__VIEWSTATE':viewState,'__EVENTVALIDATION':eventValidation,'ctl00$PageBody$ddlYear' : '2013', 'ctl00$PageBody$ddlSess':'A', 'ctl00$PageBody$ddlLevl':'U','ctl00$PageBody$ddlDept':'04'}

    #get the list of courses for Computer Science & Software Engineering
    page = requests.post('http://fcms.concordia.ca/fcms/asc002_stud_all.aspx', data=postVariables)
    response = page.text

    htmlEntities = BeautifulSoup(response)
    headings = htmlEntities.find_all("tr", {'bgcolor' : 'LightBlue'})

    hasNext = True
    headerFound = 0
    tag = headings[0]
    courses = []
    course = []
    prerequisitesToRemove = ['CWTC','BCEE','BLDG','CIVI','COEN','MECH','INDU','IADI']
    allowedCourses = ['ENGR','COMP','ENCS 282','ELEC 275','SOEN']
    while hasNext == True:
        try:
            endTest = tag.text # check if there are more tags to parse
        except:
            courses.append(course)
            course = []
            headerFound = 0
            hasNext = False
            continue
        
        if (str(tag).find('LightBlue') != -1 or str(tag).find("LightGrey") != -1) and headerFound == 1:
            headerFound = 0
            courses.append(course)
            course = []
        elif str(tag).find('LightBlue') != -1 and headerFound == 0:
            course.append(tag)
            headerFound = 1
            tag = tag.nextSibling
        elif headerFound == 1:
            course.append(tag)
            tag = tag.nextSibling
        else:
            tag = tag.nextSibling
    prerequisitePattern = re.compile('((?:[a-z][a-z][a-z]+).\\d+)',re.IGNORECASE|re.DOTALL)        
    for course in courses:
        courseHeader = course[0].findAll('b')
        courseID = courseHeader[0].text
        
        isAllowed = 0
        for allowed in allowedCourses:
            if allowed in courseID:
                isAllowed = 1
        if isAllowed == 0:
            continue
        courseDescription = courseHeader[1].text
        courseCreditAmount = courseHeader[2].text.replace(" credits","")
        if courseID in sequenceDict.keys():
            sequenceNumber = int(sequenceDict[courseID])
            del(sequenceDict[courseID])
        else:
            sequenceNumber = -1
        courseInformationMasterList[courseID] = {'Title': courseDescription,'Credits' : courseCreditAmount,'Prerequisites' : [],'Restrictions' : [], 'Course Dates' : {}, 'SemesterInSequence' : sequenceNumber}
        currentCourseLocation = 1

        if 'Prerequisite' in str(course[currentCourseLocation]):
            prerequisitesList = prerequisitePattern.findall(str(course[1]))
            for prerequisite in prerequisitesList:
                for remove in prerequisitesToRemove:
                    if remove in prerequisite:
                        prerequisitesList.remove(prerequisite)
            courseInformationMasterList[courseID]['Prerequisites'] = prerequisitesList
            currentCourseLocation += 1
        elif 'Special Note' in str(course[currentCourseLocation]): # This is not necessary but in case in the future a speacial note is added before Prerequisite text
            restrictionList = prerequisitePattern.findall(str(course[1]))
            for restriction in restrictionList:
                for remove in prerequisitesToRemove:
                    if remove in restriction:
                        restrictionList.remove(restriction)
            courseInformationMasterList[courseID]['Restrictions'] = restrictionList
            currentCourseLocation += 1
        
        if 'Special Note' in str(course[currentCourseLocation]):
            restrictionList = prerequisitePattern.findall(str(course[2]))
            for restriction in restrictionList:
                for remove in prerequisitesToRemove:
                    if remove in restriction:
                        restrictionList.remove(restriction)
            courseInformationMasterList[courseID]['Restrictions'] = restrictionList
            currentCourseLocation += 1
        elif 'Prerequisite' in str(course[currentCourseLocation]): # This is not necessary but in case in the future a Prerequisite is added before special note text
            prerequisitesList = prerequisitePattern.findall(str(course[2]))
            for prerequisite in prerequisitesList:
                for remove in prerequisitesToRemove:
                    if remove in prerequisite:
                        prerequisitesList.remove(prerequisite)
            courseInformationMasterList[courseID]['Prerequisites'] = prerequisitesList
            currentCourseLocation += 1
                   
        courseSeason = course[currentCourseLocation].findAll('b')[0].text
        courseInformationMasterList[courseID]['Course Dates'][courseSeason] = {}
        currentEntry = currentCourseLocation + 1 #We already parsed course[1] it was the season heading thats why we start at 2
        seasonToCheckFor = ['Fall','Winter']
        while currentEntry < len(course):

            #A cheaters way to identify the season. It causes issues when parsing entries that do not have the bold tag <b> which returns an empty list when we search for the tag.
            try:
                entry = course[currentEntry].findAll('b')[0].text
            except:
                entry = ""
                
            if any(season in entry for season in seasonToCheckFor):
                courseSeason = course[currentEntry].findAll('b')[0].text
                courseInformationMasterList[courseID]['Course Dates'][courseSeason] = {}
                currentEntry += 1
            else:
                courseTime = course[currentEntry].findAll('font')
                if len(courseTime) < 4:
                    currentEntry += 1
                    continue

                courseSection = courseTime[0].text.strip()
                typeOfClass = courseTime[1].text.strip()
                date = convertTimeFormat(courseTime[2].text)
                locationInfo = courseTime[3].text.strip().split(" ")
                if len(locationInfo) == 1:
                    locationInfo = [locationInfo[0], '']
                buildingLocation = {"Building" : locationInfo[0], "Room" : locationInfo[1]}
                professor = courseTime[4].text.strip()
    
                if 'Lect' in typeOfClass:
                    if 'Lecture' not in courseInformationMasterList[courseID]['Course Dates'][courseSeason].keys():
                        courseInformationMasterList[courseID]['Course Dates'][courseSeason]['Lecture'] = []
                    courseInformationMasterList[courseID]['Course Dates'][courseSeason]['Lecture'].append({'Section' : courseSection,'Dates' : date, 'Location' : buildingLocation, 'Professor' : professor})
                elif 'Tut' in typeOfClass:
                    if 'Tutorial' not in courseInformationMasterList[courseID]['Course Dates'][courseSeason].keys():
                        courseInformationMasterList[courseID]['Course Dates'][courseSeason]['Tutorial'] = []
                    courseInformationMasterList[courseID]['Course Dates'][courseSeason]['Tutorial'].append({'Section' : courseSection,'Dates' : date, 'Location' : buildingLocation})
                elif 'Lab' in typeOfClass:
                    if 'Lab' not in courseInformationMasterList[courseID]['Course Dates'][courseSeason].keys():
                        courseInformationMasterList[courseID]['Course Dates'][courseSeason]['Lab'] = []
                    courseInformationMasterList[courseID]['Course Dates'][courseSeason]['Lab'].append({'Section' : courseSection,'Dates' : date, 'Location' : buildingLocation})
                else:
                    if typeOfClass not in courseInformationMasterList[courseID]['Course Dates'][courseSeason].keys():
                        courseInformationMasterList[courseID]['Course Dates'][courseSeason][typeOfClass] = []
                    courseInformationMasterList[courseID]['Course Dates'][courseSeason][typeOfClass].append({'Section' : courseSection,'Dates' : date, 'Location' : buildingLocation, 'Professor' : professor})
                currentEntry += 1
    for speacialCourse in sequenceDict.keys():
        courseInformationMasterList[speacialCourse] = {'Title': "",'Credits' : "",'Prerequisites' : [],'Restrictions' : [], 'Course Dates' : {}, 'SemesterInSequence' : sequenceDict[speacialCourse]}
        
    with open('JSONdata.txt', 'w') as outfile:
      json.dump(courseInformationMasterList, outfile)

def getSeptSequence():
    link = 'http://www.concordia.ca/encs/computer-science-software-engineering/students/course-sequences/sept-soen-general.html'
    page = requests.get(link)
    page.encoding = 'utf-8'
    response = page.text
    
    htmlEntities = BeautifulSoup(response)
    divs = htmlEntities.find_all("div", {'class' : 'concordia-table'})
    
    heading = divs[0].find_all("tr")[1].find_all(text=True)
    heading = {heading[0]:'',heading[2]:'',heading[4]:'','SemesterInSequence': 0}

    sequenceDict = {}
    semesterNumber = 0
    
    for year in divs:
        semesters = year.find_all("th", {'colspan' : "3"})
        
        for semester in semesters:
            header = unidecode(semester.get_text())
            semester = header.split(' - ')
            
            
        semesterTable = year.find_all(["th","td"])
        currentYear = ''
        currentSemester = ''
        skipCount = 0

        for element in semesterTable:      
            if skipCount > 0:
                skipCount = skipCount - 1
                continue

            text = unidecode(element.get_text())
            output = text.replace('\n','')
            
            if 'td' in element.name and len(text) > 3:
                if output not in sequenceDict.keys():
                    sequenceDict[output] = semesterNumber
                else:
                    sequenceDict[output+str(semesterNumber)] = semesterNumber
                skipCount = 2 
    
            if 'Course Number' in output:
                skipCount = 2
                continue
            
            if ' ' == output:
                continue
            
            if 'Year' in text:
                text = text.split(" - ")
                currentYear = text[0]
                currentSemester = text[1]
                semesterNumber += 1
                continue
            
    #with open('JSONSequenceData.txt', 'w') as outfile:
      #json.dump(sequenceDict, outfile)
    return sequenceDict

      
getFallWinter(getSeptSequence())

