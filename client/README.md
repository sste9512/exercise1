# Stargate API TypeScript Client

This is an auto-generated TypeScript client library for the Stargate API.

## Installation

```bash
npm install
```

## Building

```bash
npm run build
```

## Usage

```typescript
import { AstronautDutyClient, PersonClient } from 'stargate-api-client';

// Create client instances
const astronautDutyClient = new AstronautDutyClient('https://localhost:5001');
const personClient = new PersonClient('https://localhost:5001');

// Example: Get all people
const people = await personClient.getPeople();

// Example: Get person by name
const person = await personClient.getPersonByName('John Doe');

// Example: Create a new person
await personClient.createPerson('Jane Smith');

// Example: Get astronaut duties by name
const duties = await astronautDutyClient.getAstronautDutiesByName('John Doe');

// Example: Create astronaut duty
await astronautDutyClient.createAstronautDuty({
  name: 'John Doe',
  rank: 'Captain',
  dutyTitle: 'Mission Commander',
  dutyStartDate: new Date()
});
```

## Auto-Generation

The client is automatically generated from the OpenAPI specification when you build the API project. The generated file is located at `src/api-client.ts`.

To manually regenerate the client:

1. Build the API project in Debug mode
2. The NSwag tool will automatically run and generate the TypeScript client

## Development

- `npm run build` - Compile TypeScript to JavaScript
- `npm run watch` - Watch for changes and recompile
- `npm run clean` - Remove compiled files
