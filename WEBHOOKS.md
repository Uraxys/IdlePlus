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

### Clan Action Webhooks

Clan action webhooks are triggered by various clan-related activities and provide real-time notifications about clan activities.

- **Method**: POST
- **Path**: `/clan`
- **Body Parameters**: Varies by action type (see individual actions below)

#### Available Clan Actions

The following clan action webhooks are implemented:

##### Member Management
- **`member-logged-in`** - Triggered when a clan member logs into the game
  - Parameters: `username`
- **`member-logged-out`** - Triggered when a clan member logs out of the game
  - Parameters: `username`
- **`member-joined-clan`** - Triggered when a player joins the clan
  - Parameters: `username`
- **`member-left-clan`** - Triggered when a player leaves the clan
  - Parameters: `username`
- **`member-promoted`** - Triggered when a clan member is promoted
  - Parameters: `new_rank`, `username`
- **`member-demoted`** - Triggered when a clan member is demoted
  - Parameters: `username`
- **`member-kicked`** - Triggered when a clan member is kicked from the clan
  - Parameters: `username`

##### Clan Applications
- **`application-received`** - Triggered when a player applies to join the clan
  - Parameters: `player_applying`, `player_total_level`, `message`

##### Clan Resources & Items
- **`skilling-ticket-received`** - Triggered when a clan member receives a skilling ticket
  - Parameters: `skill`, `amount`, `username`
- **`items-sent-to-vault`** - Triggered when a clan member sends multiple items to the clan vault
  - Parameters: `items` (serialized JSON array `{"<item_id>": <item_amount>}`), `username`
- **`item-withdrawn-from-vault`** - Triggered when a clan member withdraws an item from the clan vault
  - Parameters: `item_id`, `item_name`, `amount`, `username`

##### Clan Quests & Progress
- **`daily-quests-updated`** - Triggered when daily quests are refreshed
  - Parameters: None
- **`daily-combat-quest-progressed`** - Triggered when a clan member progresses a daily combat quest
  - Parameters: `entity_id`, `username`
- **`daily-skilling-quest-progressed`** - Triggered when a clan member progresses a daily skilling quest
  - Parameters: `item_id`, `item_name`, `amount`, `original_amount`, `restored_amount`, `username`

##### Clan Purchases & Upgrades
- **`clan-boss-modifier-purchased`** - Triggered when a clan boss modifier is purchased
  - Parameters: `boss_type`, `modifier_type`, `username`, `new_tier`
- **`clan-house-purchased`** - Triggered when a clan house is purchased
  - Parameters: None
- **`upgrade-purchased`** - Triggered when a clan upgrade is purchased
  - Parameters: `upgrade_type`

#### Example Clan Action Request

```
POST /clan
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
    "clientVersion": "1.5.1"
  },
  "params": {
    "action": "skilling-ticket-received",
    "skill": "Fishing",
    "amount": "1",
    "username": "PlayerName"
  }
}
```

## Adding Custom Webhook Types

Developers can contribute additional webhook types by:

1. Adding a new value to the `WebhookType` enum
2. Configuring the new webhook in the `WebhookConfigProvider._configs` dictionary
3. Calling `WebhookManager.AddSendWebhook()` at appropriate points in the code

For implementation details, see the source code in `IdlePlus/src/Utilities/WebHooks/`.