# Webhooks for Backend Developers

IdlePlus supports webhooks to integrate with external systems. This allows server-side applications to receive real-time notifications about in-game events.

## Configuration

To use webhooks:

1. Navigate to the IdlePlus settings tab in the game settings menu
2. Under "WebHooks" section:
   - Set your backend URL (must start with http:// or https://)
   - Configure a security token (will be sent as Authorization header)
   - Enable specific webhook types you want to use

## Technical Details

- Each webhook type has a predefined URL path and HTTP method
- URL paths support placeholders like `{action}` and `{type}` that get replaced with actual values
- For POST and PUT requests, metadata is automatically included in the JSON body:
  ```json
  {
    "metadata": {
      "playerName": "PlayerUsername",
      "gameMode": "Default",
      "clanName": "PlayerClan",
      "timestamp": "1743177263",
      "clientVersion": "1.4.2"
    },
    "params": {
      // Original parameters passed to the webhook
    }
    // Original request data (if any)
  }
  ```
- The Authorization header contains the security token you configured in settings

## Developer Commands

When developer tools are enabled, you can use these commands for testing webhooks:

```
/dev webhook run-test                - Run predefined webhook tests
/dev webhook show-metrics            - Display performance metrics
/dev webhook run-test-repeater       - Start automatically running tests at intervals
/dev webhook stop-test-repeater      - Stop automatic test runner
/dev webhook status-test-repeater    - Show webhook configuration status
```

## Available Webhook Types

Currently, the following webhook types are implemented:

### Minigame Events

Triggered when a player starts or stops minigame/clan events.

- **Method**: POST
- **Path**: `/minigame/{action}/{type}`
- **Path Parameters**:
  - `action`: "start" or "stop"
  - `type`: The type of minigame (e.g., "Gathering", "Crafting", "CombatBigExpDaily")

#### Example Requests

##### Start Gathering Event

```
POST /minigame/start/Gathering
Headers:
  Authorization: your-configured-token
  Content-Type: application/json; charset=utf-8

Body:
{
  "metadata": {
    "playerName": "PlayerName",
    "gameMode": "Default",
    "clanName": "ClanName",
    "timestamp": "1743177263",
    "clientVersion": "1.4.2"
  },
  "params": { 
    "action": "start", 
    "type": "Gathering" 
  }
}
```

##### Stop Combat Big Loot Daily Event

```
POST /minigame/stop/CombatBigLootDaily
Headers:
  Authorization: your-configured-token
  Content-Type: application/json; charset=utf-8

Body:
{
  "metadata": {
    "playerName": "PlayerName",
    "gameMode": "Default",
    "clanName": "ClanName",
    "timestamp": "1743178632",
    "clientVersion": "1.4.2"
  },
  "params": { 
    "action": "stop", 
    "type": "CombatBigLootDaily" 
  }
}
```

### Known Event Types

The following event types have been observed:
- `Gathering`
- `Crafting`
- `CombatBigExpDaily`
- `CombatBigLootDaily`
- `SkillingParty`

## Adding Custom Webhook Types

Developers can contribute additional webhook types by:

1. Adding a new value to the `WebhookType` enum
2. Configuring the new webhook in the `WebhookConfigProvider._configs` dictionary
3. Calling `WebhookManager.AddSendWebhook()` at appropriate points in the code

For implementation details, see the source code in `IdlePlus/src/Utilities/WebHooks/`.