# RaceDay API Endpoint Plan

This document outlines the planned RESTful API endpoints for the RaceDay event management system. The API supports two user roles: Organiser and Participant.

## Authentication

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/auth/register | Registers a new user as either an Organiser or Participant. | None (Public) | { firstName, lastName, email, password, role, phoneNumber } | 201 Created - user registered successfully. 400 Bad Request - invalid details. 409 Conflict - email already registered. |
| POST | /api/auth/login | Logs in a registered user and creates an authenticated session containing the user's ID and role. | None (Public) | { email, password } | 200 OK - login successful. 400 Bad Request - missing details. 401 Unauthorized - invalid email or password. |

## User Profile

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/users/profile | Returns the profile information of the currently logged-in user. | Any (Logged in) | None | 200 OK - profile returned. 401 Unauthorized - user is not logged in. |
| PUT | /api/users/profile | Updates the profile information of the currently logged-in user. | Any (Logged in) | { firstName, lastName, email, phoneNumber } | 200 OK - profile updated successfully. 400 Bad Request - invalid details. 401 Unauthorized - user is not logged in. 409 Conflict - email already in use. |

## Events

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/events | Returns all available events. | Any (Logged in) | None | 200 OK - list of events returned. 401 Unauthorized - user is not logged in. |
| GET | /api/events/{id} | Returns the details of a specific event. | Any (Logged in) | None | 200 OK - event returned. 401 Unauthorized - user is not logged in. 404 Not Found - event does not exist. |
| POST | /api/events | Creates a new event for the logged-in Organiser. | Organiser | { eventName, description, eventDate, location, distance, eventTypeID } | 201 Created - event created successfully. 400 Bad Request - invalid event details. 401 Unauthorized - user is not logged in. 403 Forbidden - user is not an Organiser. |
| PUT | /api/events/{id} | Updates an existing event belonging to the logged-in Organiser. | Organiser | { eventName, description, eventDate, location, distance, eventTypeID } | 200 OK - event updated successfully. 400 Bad Request - invalid event details. 401 Unauthorized - user is not logged in. 403 Forbidden - Organiser does not own the event. 404 Not Found - event does not exist. |
| DELETE | /api/events/{id} | Deletes an event belonging to the logged-in Organiser. | Organiser | None | 204 No Content - event deleted successfully. 401 Unauthorized - user is not logged in. 403 Forbidden - Organiser does not own the event. 404 Not Found - event does not exist. |

## Categories

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/events/{eventId}/categories | Returns all categories available for a specific event. | Any (Logged in) | None | 200 OK - categories returned. 401 Unauthorized - user is not logged in. |
| POST | /api/events/{eventId}/categories | Creates a new category for an event. | Organiser | { categoryName, description } | 201 Created - category created successfully. 401 Unauthorized. 403 Forbidden - user is not an Organiser. |
| PUT | /api/categories/{id} | Updates an existing category. | Organiser | { categoryName, description } | 200 OK - category updated successfully. 401 Unauthorized. 403 Forbidden. 404 Not Found. |
| DELETE | /api/categories/{id} | Deletes an existing category. | Organiser | None | 204 No Content - category deleted successfully. 401 Unauthorized. 403 Forbidden. 404 Not Found. |

## Event Enrolments

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/enrolments | Enrols the logged-in Participant in an event using a selected category. | Participant | { eventId, categoryId } | 201 Created - enrolment recorded successfully. 401 Unauthorized. 403 Forbidden - user is not a Participant. |
| GET | /api/enrolments/my | Returns all event enrolments for the logged-in Participant. | Participant | None | 200 OK - enrolments returned. 401 Unauthorized. 403 Forbidden. |
| GET | /api/events/{eventId}/enrolments | Returns all enrolments for an event belonging to the logged-in Organiser. | Organiser | None | 200 OK - enrolments returned. 401 Unauthorized. 403 Forbidden. 404 Not Found. |

## Results

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/enrolments/{enrolmentId}/result | Captures the result for a Participant after an event. | Organiser | { finishTime, finishingPosition } | 201 Created - result recorded successfully. 401 Unauthorized. 403 Forbidden - user is not an Organiser. 404 Not Found. |
| PUT | /api/results/{id} | Updates an existing Participant result. | Organiser | { finishTime, finishingPosition } | 200 OK - result updated successfully. 401 Unauthorized. 403 Forbidden. 404 Not Found. |
| GET | /api/results/my | Returns the results of the logged-in Participant. | Participant | None | 200 OK - results returned. 401 Unauthorized. 403 Forbidden. |