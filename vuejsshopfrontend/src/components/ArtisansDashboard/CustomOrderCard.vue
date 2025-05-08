<template>
  <div
      class="bg-white shadow-md rounded-2xl p-6 flex flex-col justify-between hover:shadow-xl transition duration-300 border border-gray-100"
  >
    <!-- Top Info -->
    <div class="space-y-2">
      <div class="flex items-center justify-between">
        <h2 class="text-xl font-semibold text-gray-800">Order #{{ order.id }}</h2>
        <span
            class="text-xs font-medium px-2 py-1 rounded-full"
            :class="statusBadgeClass"
        >
          {{ loading ? '...' : order.status }}
        </span>
      </div>

      <!-- Order Details -->
      <p class="text-sm text-gray-500">
        <span class="font-medium text-gray-700">Customer:</span> {{ order.name || 'N/A' }}
      </p>
      <p class="text-sm text-gray-500">
        <span class="font-medium text-gray-700">Description:</span> {{ order.description || 'N/A' }}
      </p>
      <p class="text-sm text-gray-500">
        <span class="font-medium text-gray-700">Budget:</span>
        {{ order.budget ? '$' + order.budget : 'N/A' }}
      </p>
      <p class="text-sm text-gray-500">
        <span class="font-medium text-gray-700">Deadline:</span> {{ formatDate(order.deadline) }}
      </p>
    </div>

    <!-- Bottom Actions + View Details -->
    <div class="mt-6 flex flex-wrap gap-2 justify-between items-center">
      <!-- Left: View Details -->
      <button
          @click="viewDetail"
          class="text-blue-600 hover:underline text-sm font-medium"
      >
        View Details
      </button>

      <!-- Right: Status‐dependent Actions -->
      <div class="flex flex-wrap gap-2 justify-end">
        <!-- Loading -->
        <div v-if="loading" class="flex items-center justify-center w-full">
          <span class="loader-lg"></span>
        </div>

        <!-- Pending -->
        <template v-else-if="order.status === 'pending'">
          <button
              @click="acceptOrder"
              class="bg-emerald-500 hover:bg-emerald-600 text-white text-sm px-4 py-2 rounded-lg shadow-sm transition"
          >
            Accept
          </button>
          <button
              @click="declineOrder"
              class="bg-rose-500 hover:bg-rose-600 text-white text-sm px-4 py-2 rounded-lg shadow-sm transition"
          >
            Decline
          </button>
        </template>

        <!-- Accepted & beyond (with optional Finalize) -->
        <template
            v-else-if="
            ['accepted','finalized','production','shipping','out_for_delivery','completed'].includes(order.status)
            || showMessageIcon
          "
        >
          <button
              @click="openChat"
              class="flex items-center text-blue-600 hover:text-blue-800 text-sm font-medium transition"
          >
            <svg
                class="w-5 h-5 mr-1"
                fill="none"
                stroke="currentColor"
                stroke-width="2"
                viewBox="0 0 24 24"
            >
              <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  d="M8 10h.01M12 10h.01M16 10h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
              />
            </svg>
            Message
          </button>
          <button
              v-if="order.status === 'accepted'"
              @click="finalizeOrder"
              class="bg-indigo-500 hover:bg-indigo-600 text-white text-sm px-4 py-2 rounded-lg shadow-sm transition"
          >
            Finalize
          </button>
        </template>

        <!-- Rejected (allow re-accept) -->
        <template v-else-if="order.status === 'rejected'">
          <button
              @click="handleReAccept"
              class="bg-emerald-500 hover:bg-emerald-600 text-white text-sm px-4 py-2 rounded-lg shadow-sm transition"
          >
            Accept
          </button>
        </template>
      </div>
    </div>
  </div>
</template>

<script setup>
import { defineProps, defineEmits, ref, computed } from 'vue'

const props = defineProps({
  order: { type: Object, required: true },
  loading: { type: Boolean, default: false }
})

const emit = defineEmits([
  'accept',
  'decline',
  'chat',
  'finalize',
  'view-detail'    // <— new event
])

const showMessageIcon = ref(false)

const acceptOrder   = () => emit('accept', props.order.id)
const declineOrder  = () => emit('decline', props.order.id)
const openChat      = () => emit('chat', props.order.id)
const finalizeOrder = () => emit('finalize', props.order.id)
const viewDetail    = () => emit('view-detail', props.order.id)  // <— new handler

const handleReAccept = () => {
  emit('accept', props.order.id)
  showMessageIcon.value = true
}

const formatDate = dateStr =>
    dateStr ? new Date(dateStr).toLocaleDateString() : '—'

const statusBadgeClass = computed(() => {
  if (props.loading) return 'bg-gray-300 text-gray-600'
  return {
    pending:   'bg-yellow-100 text-yellow-800',
    accepted:  'bg-green-100  text-green-800',
    rejected:  'bg-red-100    text-red-800',
    finalized: 'bg-indigo-100 text-indigo-800'
  }[props.order.status] || 'bg-gray-200 text-gray-700'
})
</script>

<style scoped>
.loader-lg {
  border: 4px solid #f3f3f3;
  border-top: 4px solid #3498db;
  border-radius: 50%;
  width: 24px;
  height: 24px;
  animation: spin 1s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg) } }
</style>
